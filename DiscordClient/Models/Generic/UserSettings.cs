using System.Text.Json.Serialization;

namespace DiscordClient.Models.Generic;

public class UserSettings
{
    [JsonPropertyName("locale")]
    public string Locale { get; set; }

    [JsonPropertyName("theme")]
    public string Theme { get; set; }
}