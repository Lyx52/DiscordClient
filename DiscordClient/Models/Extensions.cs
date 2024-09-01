using DiscordClient.Models.interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DiscordClient.Models;
public static class Extensions
{
    public static IServiceCollection AddRestClient<THttpClient>(this IServiceCollection services) where THttpClient : class
    {
        services.AddScoped<THttpClient>((provider) =>
        {
            var handler = provider.GetService<IRestClientHandler>();
            return handler!.GetHttpClientInstance<THttpClient>();
        });
        services.Configure<RestClientConfiguration>((config) => config.AddHttpClient<THttpClient>());
        return services;
    }
    
    public static IServiceCollection AddRestClientHandler(this IServiceCollection services, string handlerName = "DefaultClient", Action<RestClientConfiguration>? configurator = null)
    {
        if (configurator is not null) services.Configure<RestClientConfiguration>(configurator); 
        services.AddSingleton<IRestClientHandler, RestClientHandler>();
        return services;
    }
}