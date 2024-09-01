using System.Reflection.Emit;
using Microsoft.Extensions.Options;

namespace DiscordClient.Models;

public enum ProxyArgType
{
    ArgValue,
    RawValue
}

public record InputParameter(Type ValueType, ProxyArgType Type, dynamic? Value);
public class ProxyMethodInputParameterBundle
{
    private Dictionary<string, InputParameter> _parameters;
    private List<string> _parameterKeyOrder;
    public ProxyMethodInputParameterBundle()
    {
        _parameterKeyOrder = new List<string>();
        _parameters = new Dictionary<string, InputParameter>();
    }

    public TValue GetValueByName<TValue>(object[] args, string name)
    {
        var argIndex = _parameterKeyOrder.IndexOf(name);
        if (argIndex < 0) 
            throw new ArgumentException($"ProxyParameter with name {name} does not exist!");
        return (TValue)args[argIndex];
    }
    public dynamic? GetValueByName(object[] args, string name)
    {
        var argIndex = _parameterKeyOrder.IndexOf(name);
        if (argIndex < 0)
            return default!;
        return args[argIndex];
    }

    public (string Name, string Value)[] GetHeaderParameters(object[] args)
    {
        var headerKeys = _parameterKeyOrder.Where(pk => pk.StartsWith("header"));

        return headerKeys.Select(hk => 
            (hk.Replace("header:", string.Empty), (string)args[_parameterKeyOrder.IndexOf(hk)])
        ).ToArray();
    }
    public ProxyMethodInputParameterBundle AddRaw(dynamic value, string valueName)
    {
        return Add(value.GetType(), ProxyArgType.RawValue, valueName, value);
    }

    public ProxyMethodInputParameterBundle AddArg(params (Type, string)[] args)
    {
        foreach (var (type, name) in args)
        {
            Add(type, ProxyArgType.ArgValue, name);    
        }

        return this;
    }
    public ProxyMethodInputParameterBundle Add(Type paramType, ProxyArgType type, string valueName, dynamic? value = null)
    {
        _parameters.Add(valueName, new InputParameter(paramType, type, value));
        return this;
    }

    public ProxyMethodInputParameterBundle BuildIlCode(ILGenerator il)
    {
        // Create new params object[] for input parameter with length of method parameter count
        il.Emit(OpCodes.Ldc_I4, _parameters.Count);
        il.Emit(OpCodes.Newarr, typeof(object));
        int i = 0;

        foreach (var (name, param) in _parameters)
        {
            // Load value and param array
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4, i);

            switch (param.Type)
            {
                case ProxyArgType.ArgValue:
                {
                    il.Emit(OpCodes.Ldarg, i + 1);    
                } break;
                case ProxyArgType.RawValue:
                {
                    EmitLoadDynamic(il, param.Value);
                } break;
            }
            
            // Box value and store it in params array
            il.Emit(OpCodes.Box, param.ValueType);
            il.Emit(OpCodes.Stelem_Ref);
            i++;
            _parameterKeyOrder.Add(name);
        }

        return this;
    }

    private void EmitParameter(string name, InputParameter param)
    {
        
    }
    private ProxyMethodInputParameterBundle EmitLoadDynamic(ILGenerator il, dynamic value)
    {
        switch (value)
        {
            case string stringValue:
            {
                il.Emit(OpCodes.Ldstr, stringValue);
            } break;
            case int intValue:
            {
                il.Emit(OpCodes.Ldc_I4, intValue);
            } break;
            default:
                throw new ArgumentException($"Unsupported dynamic type {value.GetType()}");
        }
        return this;
    }
}