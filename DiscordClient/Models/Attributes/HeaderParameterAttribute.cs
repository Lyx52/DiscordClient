namespace DiscordClient.Models.Attributes;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class HeaderParameterAttribute : Attribute
{
    public string Name { get; set; }

    public HeaderParameterAttribute(string parameterName)
    {
        Name = parameterName;
    }
}