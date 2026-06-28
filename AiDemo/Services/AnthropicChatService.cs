using System.Text;
using System.Text.Json;
using AiDemo.Models;

namespace AiDemo.Services;

public class AnthropicChatService
{
    private readonly ConfigurationService _configService;
    private readonly McpToolService _toolService;
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public AnthropicChatService(ConfigurationService configService, McpToolService toolService, HttpClient httpClient)
    {
        _configService = configService;
        _toolService = toolService;
        _httpClient = httpClient;
    }

    public async Task<ChatMessage> SendMessageAsync(List<ChatMessage> history, string userMessage)
    {
        var config = _configService.GetConfig();

        if (string.IsNullOrWhiteSpace(config.ApiKey))
        {
            return new ChatMessage
            {
                Role = "assistant",
                Content = "⚠️ Bitte konfigurieren Sie zuerst den API-Key in den Einstellungen (Systemprompt-Seite)."
            };
        }

        // Build Anthropic messages from history
        var messages = new List<AnthropicMessage>();
        foreach (var msg in history.Where(m => m.Role is "user" or "assistant"))
        {
            messages.Add(new AnthropicMessage { Role = msg.Role, Content = msg.Content });
        }
        messages.Add(new AnthropicMessage { Role = "user", Content = userMessage });

        // Build tools from MCP server configs
        var tools = BuildToolList(config);

        // Create request
        var request = new AnthropicRequest
        {
            Model = config.Model,
            MaxTokens = 4096,
            System = string.IsNullOrWhiteSpace(config.SystemPrompt) ? null : config.SystemPrompt,
            Messages = messages,
            Tools = tools.Count > 0 ? tools : null
        };

        var result = new ChatMessage
        {
            Role = "assistant",
            ToolCalls = [],
            Documents = []
        };

        // Conversation loop for tool use
        var currentMessages = new List<AnthropicMessage>(messages);
        var maxIterations = 10;

        for (int i = 0; i < maxIterations; i++)
        {
            var response = await CallAnthropicApi(new AnthropicRequest
            {
                Model = config.Model,
                MaxTokens = 4096,
                System = request.System,
                Messages = currentMessages,
                Tools = request.Tools
            });

            if (response.Error != null)
            {
                result.Content = $"API-Fehler: {response.Error.Message}";
                return result;
            }

            // Process response content blocks
            var textParts = new StringBuilder();
            var toolUseBlocks = new List<AnthropicContentBlock>();

            foreach (var block in response.Content)
            {
                if (block.Type == "text" && block.Text != null)
                {
                    textParts.AppendLine(block.Text);
                }
                else if (block.Type == "tool_use")
                {
                    toolUseBlocks.Add(block);
                }
            }

            if (toolUseBlocks.Count == 0)
            {
                // No tool calls, we're done
                result.Content += textParts.ToString();
                break;
            }

            // Add assistant response to messages
            var assistantContent = response.Content.Select(b =>
            {
                if (b.Type == "text")
                    return (object)new { type = "text", text = b.Text };
                return (object)new { type = "tool_use", id = b.Id, name = b.Name, input = b.Input };
            }).ToList();

            currentMessages.Add(new AnthropicMessage { Role = "assistant", Content = assistantContent });

            // Execute tools and collect results
            var toolResults = new List<object>();
            foreach (var toolBlock in toolUseBlocks)
            {
                var toolInput = toolBlock.Input ?? default;
                var (toolResult, document) = await _toolService.ExecuteToolAsync(
                    toolBlock.Name ?? "", toolInput);

                result.ToolCalls!.Add(new ToolCall
                {
                    ToolName = toolBlock.Name ?? "",
                    Input = toolInput.ToString() ?? "{}",
                    Output = toolResult
                });

                if (document != null)
                    result.Documents!.Add(document);

                toolResults.Add(new
                {
                    type = "tool_result",
                    tool_use_id = toolBlock.Id,
                    content = toolResult
                });
            }

            currentMessages.Add(new AnthropicMessage { Role = "user", Content = toolResults });

            // Add any text before tool calls
            if (textParts.Length > 0)
                result.Content += textParts.ToString();
        }

        return result;
    }

    private async Task<AnthropicResponse> CallAnthropicApi(AnthropicRequest request)
    {
        var config = _configService.GetConfig();

        var jsonContent = JsonSerializer.Serialize(request, JsonOptions);
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages")
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Add("x-api-key", config.ApiKey);
        httpRequest.Headers.Add("anthropic-version", "2023-06-01");

        var httpResponse = await _httpClient.SendAsync(httpRequest);
        var responseJson = await httpResponse.Content.ReadAsStringAsync();

        if (!httpResponse.IsSuccessStatusCode)
        {
            return new AnthropicResponse
            {
                Error = new AnthropicError
                {
                    Type = "api_error",
                    Message = $"HTTP {(int)httpResponse.StatusCode}: {responseJson}"
                }
            };
        }

        return JsonSerializer.Deserialize<AnthropicResponse>(responseJson, JsonOptions)
               ?? new AnthropicResponse { Error = new AnthropicError { Message = "Leere Antwort von der API" } };
    }

    private static List<AnthropicTool> BuildToolList(AppConfig config)
    {
        var tools = new List<AnthropicTool>();

        foreach (var server in config.McpServers.Where(s => s.Enabled))
        {
            foreach (var tool in server.Tools)
            {
                var schema = new AnthropicToolSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, AnthropicPropertySchema>(),
                    Required = []
                };

                foreach (var (paramName, param) in tool.Parameters)
                {
                    schema.Properties[paramName] = new AnthropicPropertySchema
                    {
                        Type = param.Type,
                        Description = param.Description
                    };
                    if (param.Required)
                        schema.Required.Add(paramName);
                }

                tools.Add(new AnthropicTool
                {
                    Name = tool.Name,
                    Description = tool.Description,
                    InputSchema = schema
                });
            }
        }

        return tools;
    }
}
