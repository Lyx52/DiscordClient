using System.Text.Json.Serialization;
using DiscordClient.Models.Generic;

namespace DiscordClient.Models.Response;

public class UserInfoResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("username")]
    public string Username { get; set; }
    
    [JsonPropertyName("global_name")]
    public string GlobalName { get; set; }
    
    [JsonPropertyName("discriminator")]
    public string Discriminator { get; set; }
    
    [JsonPropertyName("public_flags")]
    public int PublicFlags { get; set; }
    
    [JsonPropertyName("flags")]
    public int Flags { get; set; }
    
    [JsonPropertyName("premium_type")]
    public int PremiumType { get; set; }
    
    [JsonPropertyName("banner")]
    public string? Banner { get; set; }
    
    [JsonPropertyName("accent_color")]
    public string? AccentColor { get; set; }
    
    [JsonPropertyName("banner_color")]
    public string? BannerColor { get; set; }
    
    [JsonPropertyName("clan")]
    public string? Clan { get; set; }
    
    [JsonPropertyName("email")]
    public string Email { get; set; }
    
    [JsonPropertyName("phone")]
    public string Phone { get; set; }
    
    [JsonPropertyName("locale")]
    public string Locale { get; set; }
    
    [JsonPropertyName("bio")]
    public string Bio { get; set; }
    
    [JsonPropertyName("nsfw_allowed")]
    public bool NsfwAllowed { get; set; }
    
    [JsonPropertyName("verified")]
    public bool Verified { get; set; }
    
    [JsonPropertyName("mfa_enabled")]
    public bool MfaEnabled { get; set; }
    
    [JsonPropertyName("avatar_decoration_data")]
    public AvatarDecorationData? AvatarDecorationData { get; set; }
}