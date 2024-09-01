namespace DiscordClient.Models.interfaces;

public interface IRestClientHandler
{
    RestClientHandler AddHttpClient<THttpClient>() where THttpClient : class;
    THttpClient GetHttpClientInstance<THttpClient>() where THttpClient : class;
}