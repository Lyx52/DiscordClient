namespace DiscordClient.Models.Attributes;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class ParameterNameAttribute : Attribute
{
    public string Name { get; set; }
    public ParameterNameAttribute(string name)
    {
        Name = name;
    }
}