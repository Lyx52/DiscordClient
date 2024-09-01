using System.Text.Json.Serialization;

namespace DiscordClient.Models.Generic;

public class AvatarDecorationData
{
    [JsonPropertyName("asset")]
    public string? Asset { get; set; }
    
    [JsonPropertyName("sku_id")]
    public string? SkuId { get; set; }
    
    [JsonPropertyName("expires_at")]
    public string? ExpiresAt { get; set; }
}