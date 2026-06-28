namespace AiDemo.Models;

public class AppConfig
{
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "claude-3-5-sonnet-latest";
    public string SystemPrompt { get; set; } = "";
    public List<McpServerConfig> McpServers { get; set; } = [];
}

public class McpServerConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool Enabled { get; set; } = true;
    public List<McpToolConfig> Tools { get; set; } = [];
}

public class McpToolConfig
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public Dictionary<string, ToolParameter> Parameters { get; set; } = new();
}

public class ToolParameter
{
    public string Type { get; set; } = "string";
    public string Description { get; set; } = "";
    public bool Required { get; set; } = true;
}
