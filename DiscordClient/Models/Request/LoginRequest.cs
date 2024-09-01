using System.Text.Json.Serialization;
using DiscordClient.Models.Response;

namespace DiscordClient.Models.Request;

public class LoginRequest
{
    [JsonPropertyName("login")]
    public string Login { get; set; }
    
    [JsonPropertyName("password")]
    public string Password { get; set; }
    
    [JsonPropertyName("undelete")]
    public bool Undelete { get; set; }
    
    [JsonPropertyName("login_source")]
    public string? LoginSource { get; set; }
    
    [JsonPropertyName("gift_code_sku_id")]
    public string? GiftCodeSkuId { get; set; }

    public LoginRequest(string login, string password, bool undelete = false, string? loginSource = null, string? giftCodeSkuId = null)
    {
        Login = login;
        Password = password;
        Undelete = undelete;
        LoginSource = loginSource;
        GiftCodeSkuId = giftCodeSkuId;
    }
}