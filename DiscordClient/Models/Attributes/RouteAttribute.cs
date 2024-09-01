namespace DiscordClient.Models.Attributes;
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RouteAttribute : Attribute
{
    public string Route { get; set; }
    public string Method { get; set; }
    public RouteAttribute(string method, string route)
    {
        Route = route;
        Method = method;
    }
}