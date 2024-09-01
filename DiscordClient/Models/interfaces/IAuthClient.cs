using DiscordClient.Models.Attributes;
using DiscordClient.Models.Request;
using DiscordClient.Models.Response;

namespace DiscordClient.Models.interfaces;

public interface IAuthClient
{
    [Route(HttpMethodType.Post, "api/v9/auth/login")]
    public Task<LoginResponse> LoginAsync([ParameterName("body")]LoginRequest request);
}