using System.Text.Json.Serialization;

namespace AiDemo.Models;

// --- Request Models ---

public class AnthropicRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "claude-3-5-sonnet-latest";

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; } = 4096;

    [JsonPropertyName("system")]
    public string? System { get; set; }

    [JsonPropertyName("messages")]
    public List<AnthropicMessage> Messages { get; set; } = [];

    [JsonPropertyName("tools")]
    public List<AnthropicTool>? Tools { get; set; }
}

public class AnthropicMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "";

    [JsonPropertyName("content")]
    public object Content { get; set; } = "";
}

public class AnthropicTool
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("input_schema")]
    public AnthropicToolSchema InputSchema { get; set; } = new();
}

public class AnthropicToolSchema
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "object";

    [JsonPropertyName("properties")]
    public Dictionary<string, AnthropicPropertySchema> Properties { get; set; } = new();

    [JsonPropertyName("required")]
    public List<string> Required { get; set; } = [];
}

public class AnthropicPropertySchema
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "string";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";
}

// --- Response Models ---

public class AnthropicResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("role")]
    public string Role { get; set; } = "";

    [JsonPropertyName("content")]
    public List<AnthropicContentBlock> Content { get; set; } = [];

    [JsonPropertyName("stop_reason")]
    public string? StopReason { get; set; }

    [JsonPropertyName("error")]
    public AnthropicError? Error { get; set; }
}

public class AnthropicContentBlock
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = ""; // "text" or "tool_use"

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("input")]
    public System.Text.Json.JsonElement? Input { get; set; }
}

public class AnthropicError
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
}

// Tool result content block for sending back tool results
public class ToolResultContent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "tool_result";

    [JsonPropertyName("tool_use_id")]
    public string ToolUseId { get; set; } = "";

    [JsonPropertyName("content")]
    public string Content { get; set; } = "";
}
