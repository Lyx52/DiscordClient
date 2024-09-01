using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;
using System.Text.RegularExpressions;
using DiscordClient.Models;
using DiscordClient.Models.Attributes;
using DiscordClient.Models.interfaces;
using DiscordClient.Models.Request;
using DiscordClient.Models.Response;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions;
namespace DiscordClient;

public class RestClientHandler : IRestClientHandler
{
    private static readonly Dictionary<string, Dictionary<string, ProxyMethodInputParameterBundle>> ProxyInterfaceImplementations = new();
    private static readonly MethodInfo HandleInterfaceMethodResultInfo = typeof(RestClientHandler).GetMethod(nameof(HandleInterfaceMethod))!;
    private Dictionary<Type, dynamic> _httpClientInstances;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly RestClientConfiguration _configuration;
    public RestClientHandler(IServiceScopeFactory serviceScopeFactory, IOptions<RestClientConfiguration> options)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = options.Value;
        _httpClientInstances = new Dictionary<Type, dynamic>();
        options.Value.ConfigureClient(this);
    }
    public RestClientHandler AddHttpClient<THttpClient>() where THttpClient : class
    {
        var client = CreateClientInterfaceProxy<THttpClient>();
        if (client is null)
            throw new ApplicationException($"Failed to create HttpClient instance for {typeof(THttpClient).Name}");
        _httpClientInstances.Add(typeof(THttpClient), client);
        return this;
    }


    public THttpClient GetHttpClientInstance<THttpClient>() where THttpClient : class
    {
        if (_httpClientInstances.TryGetValue(typeof(THttpClient), out var client))
        {
            return (THttpClient)client;
        }

        throw new ApplicationException($"HttpClient {typeof(THttpClient).Name} does not exist!");
    }
    
    private TInterface? CreateClientInterfaceProxy<TInterface>()
    {
        var interfaceType = typeof(TInterface);

        if (!interfaceType.IsInterface)
        {
            throw new ArgumentException("Generic type must be an interface");
        }
        
        var assemblyName = new AssemblyName($"{interfaceType.Name}_ProxyAssembly");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule($"{interfaceType.Name}_ProxyModule");
        var typeBuilder = moduleBuilder.DefineType($"{interfaceType.Name}_ProxyType", TypeAttributes.Public);
        
        // Implement interface
        var bundle = ImplementImplementInterfaceType(typeBuilder, interfaceType);
        ProxyInterfaceImplementations.Add(interfaceType.FullName!, bundle);
        var proxyType = typeBuilder.CreateType();
        return (TInterface?)Activator.CreateInstance(proxyType);
    }
    private Dictionary<string, ProxyMethodInputParameterBundle> ImplementImplementInterfaceType(TypeBuilder typeBuilder, Type interfaceType)
    {
        typeBuilder.AddInterfaceImplementation(interfaceType);
        var bundles = new Dictionary<string, ProxyMethodInputParameterBundle>();
        foreach (var method in interfaceType.GetMethods())
        {
            if (bundles.ContainsKey(method.Name))
                throw new ArgumentException($"Method with name {method.Name} already exists!");
            var bundle = DefineMethodImplementation(typeBuilder, method);
            bundles.Add(method.Name, bundle);
        }

        return bundles;
    }
    private ProxyMethodInputParameterBundle DefineMethodImplementation(TypeBuilder typeBuilder, MethodInfo interfaceMethod)
    {
        var methodBuilder = typeBuilder.DefineMethod(
            $"{interfaceMethod.Name}_ProxyMethod",
            MethodAttributes.Private | MethodAttributes.HideBySig |
            MethodAttributes.NewSlot | MethodAttributes.Virtual |
            MethodAttributes.Final,
            interfaceMethod.ReturnType,
            interfaceMethod.GetParameters().Select(p => p.ParameterType).ToArray()
        );
        var bundle = BuildMethodBody(methodBuilder, interfaceMethod);
        typeBuilder.DefineMethodOverride(methodBuilder, interfaceMethod);
        return bundle;
    }
    private ProxyMethodInputParameterBundle BuildMethodBody(MethodBuilder proxyMethodBuild, MethodInfo interfaceMethod)
    {
        var il = proxyMethodBuild.GetILGenerator();
        var parameters = interfaceMethod.GetParameters();
        var routeAttribute = interfaceMethod.GetCustomAttribute<RouteAttribute>()!;
        var bundle = new ProxyMethodInputParameterBundle()
            .AddArg(BuildArgParameters(parameters))
            .AddRaw(routeAttribute.Method, "method")
            .AddRaw(routeAttribute.Route, "route")
            .AddRaw(_configuration.Hostname, "baseuri")
            .AddRaw(interfaceMethod.DeclaringType!.ToString(), "proxyDeclaringType")
            .AddRaw(interfaceMethod.Name, "proxyMethodName")
            .BuildIlCode(il);

        if (interfaceMethod.ReturnType == typeof(void))
            throw new ArgumentException("Interface methods with return type void are not supported!");
        il.Emit(OpCodes.Call,  HandleInterfaceMethodResultInfo.MakeGenericMethod(WrapResultWithTask(interfaceMethod.ReturnType), UnwrapTaskResult(interfaceMethod.ReturnType)));
        
        il.Emit(OpCodes.Ret);
        return bundle;
    }

    private (Type ValueType, string Name)[] BuildArgParameters(ParameterInfo[] parameters)
    {
        var args = new List<(Type ValueType, string Name)>();
        var i = 0;
        foreach (var param in parameters)
        {
            var headerParamAttrib = param.GetCustomAttribute<HeaderParameterAttribute>();
            var parameterName = param.GetCustomAttribute<ParameterNameAttribute>();
            if (headerParamAttrib is not null)
            {
                args.Add((param.ParameterType, $"header:{headerParamAttrib.Name}"));
                continue;
            }
            args.Add((param.ParameterType, parameterName?.Name ?? $"arg{i++}"));
        }
        return args.ToArray();
    }
    private static Type WrapResultWithTask(Type type)
    {
        return type.BaseType == typeof(Task) ? type : Type.MakeGenericSignatureType(typeof(Task), type);
    }
    private static Type UnwrapTaskResult(Type type)
    {
        return type.BaseType == typeof(Task) ? type.GenericTypeArguments.First() : type;
    }
    private static ProxyMethodInputParameterBundle GetBundle(object[] args)
    {
        var proxyMethodName = (string)args[^1];
        var proxyMethodDeclaringType = (string)args[^2];
        return ProxyInterfaceImplementations[proxyMethodDeclaringType][proxyMethodName];
    }
    public static TTaskResult HandleInterfaceMethod<TTaskResult, TResultType>(params object[] args)
    where TTaskResult : Task<TResultType>
    {
        var methodBundle = GetBundle(args);
        var route = methodBundle.GetValueByName<string>(args, "route")!;
        var routeParameters = Regex.Matches(route, @"\{@?\w+}");
        foreach (Match match in routeParameters)
        {
            var paramName = match.Value[1..^1];
            var param = methodBundle.GetValueByName<string?>(args, paramName);
            if (param is null)
            {
                throw new ArgumentException($"Route parameter {paramName} is not supplied but required by route ${route}");
            }

            route = route.Replace(match.Value, param);
        }
        
        var baseuri = methodBundle.GetValueByName<string>(args, "baseuri")!;
        var method = methodBundle.GetValueByName<string>(args, "method")!;
        var body = methodBundle.GetValueByName(args, "body");
        var queryParams = methodBundle.GetValueByName(args, "queryParams");
        var headerParams = methodBundle.GetHeaderParameters(args);
        var httpClient = new HttpClient();
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri(new Uri(baseuri), route),
            Method = HttpMethod.Parse(method)
        };
        foreach (var param in headerParams)
        {
            request.Headers.Add(param.Name, param.Value);
        }
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }
        var response = httpClient.Send(request);
        response.EnsureSuccessStatusCode();
        var result = JsonSerializer.Deserialize(response.Content.ReadAsStream(), typeof(TResultType));
        return (TTaskResult)Task.FromResult((TResultType)result!);
    }
}