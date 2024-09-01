using DiscordClient.Models.Attributes;
using DiscordClient.Models.Request;
using DiscordClient.Models.Response;

namespace DiscordClient.Models.interfaces;

public interface IUsersClient
{
    [Route(HttpMethodType.Get, "api/v9/users/{userId}")]
    public Task<UserInfoResponse> GetUserInfoAsync([ParameterName("userId")] string userId, [HeaderParameter("Authorization")] string token);
}