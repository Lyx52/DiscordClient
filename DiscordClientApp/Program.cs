using System.Net.Http.Json;
using System.Reflection;
using System.Reflection.Emit;
using DiscordClient.Models;
using DiscordClient.Models.interfaces;
using DiscordClient.Models.Request;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace DiscordClient.Test;


// Example usage
public class Program
{
    public static async Task Main()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddRestClientHandler("RestClient", (config) => config.WithHostname("https://discord.com"));
        builder.Services.AddRestClient<IAuthClient>();
        builder.Services.AddRestClient<IUsersClient>();
        builder.Services.AddRestClient<IChannelsClient>();
        var app = builder.Build();
        await Task.WhenAll(
            Test(app),
            app.RunAsync());
    }

    public static async Task Test(IHost app)
    {
        var authClient = app.Services.GetRequiredService<IAuthClient>();
        var usersClient = app.Services.GetRequiredService<IChannelsClient>();
        
        var res = await authClient.LoginAsync(new LoginRequest("username", "passs"));
        var userInfo = await usersClient.SendMessageAsync("xxx", new SendMessageRequest{ Content = $"Hi hello! {Guid.NewGuid()}" }, res.Token);
    }
}