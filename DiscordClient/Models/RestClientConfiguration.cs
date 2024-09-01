namespace DiscordClient.Models;

public class RestClientConfiguration
{
    private List<Action<RestClientHandler>> _configurations { get; set; } = new List<Action<RestClientHandler>>();
    public string Hostname { get; private set; }
    public RestClientConfiguration AddHttpClient<THttpClient>() where THttpClient : class
    {
        _configurations.Add(handler => handler.AddHttpClient<THttpClient>());
        return this;
    }

    public RestClientConfiguration WithHostname(string hostname)
    {
        Hostname = hostname;
        return this;
    }
    public void ConfigureClient(RestClientHandler handler)
    {
        try
        {
            foreach (var configuration in _configurations)
            {
                configuration.Invoke(handler);
            }
        }
        catch (Exception e)
        {
            throw new ApplicationException("Failed to configure rest clients", e);
        }
    }
}