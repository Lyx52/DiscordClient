using System.Text.Json.Serialization;
using DiscordClient.Models.Generic;

namespace DiscordClient.Models.Response;

public class LoginResponse
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }
    
    [JsonPropertyName("token")]
    public string Token { get; set; }
    
    [JsonPropertyName("user_settings")]
    public UserSettings UserSettings { get; set; }
}