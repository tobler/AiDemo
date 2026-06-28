namespace AiDemo.Models;

public class ChatMessage
{
    public string Role { get; set; } = "user"; // user, assistant, system
    public string Content { get; set; } = "";
    public List<ToolCall>? ToolCalls { get; set; }
    public List<GeneratedDocument>? Documents { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public class ToolCall
{
    public string ToolName { get; set; } = "";
    public string Input { get; set; } = "";
    public string Output { get; set; } = "";
}

public class GeneratedDocument
{
    public string FileName { get; set; } = "";
    public string FileType { get; set; } = ""; // pdf, docx, xlsx
    public byte[] Data { get; set; } = [];
    public string DownloadUrl { get; set; } = "";
}
