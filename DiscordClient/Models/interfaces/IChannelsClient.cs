using DiscordClient.Models.Attributes;
using DiscordClient.Models.Request;
using DiscordClient.Models.Response;

namespace DiscordClient.Models.interfaces;

public interface IChannelsClient
{
    [Route(HttpMethodType.Post, "api/v9/channels/{channelId}/messages")]
    public Task<UserInfoResponse> SendMessageAsync([ParameterName("channelId")] string channelId, [ParameterName("body")] SendMessageRequest request, [HeaderParameter("Authorization")] string token);
    
    [Route(HttpMethodType.Get, "api/v9/channels/{channelId}/messages")]
    public Task<UserInfoResponse> GetMessagesAsync([ParameterName("channelId")] string channelId, [HeaderParameter("Authorization")] string token);
}