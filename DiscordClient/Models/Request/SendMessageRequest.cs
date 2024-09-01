using System.Text.Json.Serialization;

namespace DiscordClient.Models.Request;

public class SendMessageRequest
{
    [JsonPropertyName("content")]
    public string Content { get; set; } 
}