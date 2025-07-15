using MedihatR.Engine.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace MedihatR;
public static class RegisterServicesExt
{
    private static bool _scanned;

    private static Type _requestHandlerVoidType = typeof(IRequestHandler<>);
    private static Type _requestHandlerType = typeof(IRequestHandler<,>);
    private static Type _notificationHandlerType = typeof(INotificationHandler<>);
    public static MedihaterConfiguration Configuration { get; private set; } = new MedihaterConfiguration();
    public static IServiceCollection AddMedihaterServices(this IServiceCollection services, Action<MedihaterConfiguration>? configure = default)
    {
        if (configure != default)
            configure(Configuration);

        if (Configuration.AssembliesScan.Any())
            services.ScanAssemblies(Configuration.AssembliesScan.ToArray());

        services.AddScoped<IMedihater, Medihater>();
        return services;
    }

    private static void ScanAssemblies(this IServiceCollection services, params Type[] assemblies)
    {
        if (_scanned) return;
        _scanned = true;

        foreach (var type in assemblies.SelectMany(p => p.Assembly.GetTypes()))
        {
            if (type.IsAbstract || type.IsInterface)
                continue;

            var interfaces = type.GetInterfaces();
            foreach (var iface in interfaces)
                services
                    .TryRegisterNotificationHandlers(iface, type)
                    .TryRegisterRequestHandlers(iface, type);
        }
    }
    public static IServiceCollection AddMedihaterRequestHandler<TRequest, THandler, TResult>(this IServiceCollection services)
        where TRequest : IRequest<TResult>
        where THandler : IRequestHandler<TRequest, TResult>
    {
        Type impleType = typeof(THandler);
        Type requestType = typeof(TRequest);
        Type tResultType = typeof(TResult);
        Type iface = _requestHandlerType.MakeGenericType(requestType, tResultType);
        services.TryRegisterRequestHandlers(iface, impleType);
        return services;

    }
    public static IServiceCollection AddMedihaterRequestHandler<TRequest, THandler>(this IServiceCollection services)
        where TRequest : IRequest
        where THandler : IRequestHandler<TRequest>
    {
        Type impleType = typeof(THandler);
        Type requestType = typeof(TRequest);
        Type iface = _requestHandlerVoidType.MakeGenericType(requestType);
        services.TryRegisterRequestHandlers(iface, impleType);
        return services;
    }
    public static IServiceCollection AddMedihaterNotificationHandler<TNotification, THandler>(this IServiceCollection services)
        where TNotification : INotification
        where THandler : INotificationHandler<TNotification>
    {
        Type impleType = typeof(THandler);
        Type notificationType = typeof(TNotification);
        Type iface = _requestHandlerType.MakeGenericType(notificationType);
        services.TryRegisterRequestHandlers(iface, impleType);
        return services;

    }

    private static IServiceCollection TryRegisterNotificationHandlers(this IServiceCollection services, Type iFace, Type implementation)
    {
        if (iFace.IsGenericType &&
            !services.IsImplementationRegistered(implementation, iFace) &&
            iFace.GetGenericTypeDefinition() == _notificationHandlerType
            )
            services.AddTransient(iFace, implementation);
        return services;
    }

    private static IServiceCollection TryRegisterRequestHandlers(this IServiceCollection services, Type iFace, Type implementation)
    {
        bool isGenericAndDefinitionRequestType = iFace.IsGenericType && (iFace.GetGenericTypeDefinition() == _requestHandlerType || iFace.GetGenericTypeDefinition() == _requestHandlerVoidType);
        bool isNonGenericAndRequestType = !iFace.IsGenericType && (iFace == _requestHandlerType || iFace == _requestHandlerVoidType);
        bool shouldRegister = (isGenericAndDefinitionRequestType || isNonGenericAndRequestType) &&
            !services.IsServiceInterfaceRegistered(iFace);
        if (shouldRegister)
            services.AddTransient(iFace, implementation);
        return services;
    }

    public static bool IsServiceInterfaceRegistered(this IServiceCollection services, Type iface)
    {
        return services.Any(s => s.ServiceType == iface);
    }
    public static bool IsImplementationRegistered(this IServiceCollection services, Type implementationType, Type ifaceType)
    {
        return services.Any(s =>
            s.ImplementationType == implementationType &&
            s.ServiceType.IsGenericType &&
            s.ServiceType.GetGenericTypeDefinition() == ifaceType ||
            !s.ServiceType.IsGenericType && s.ServiceType == ifaceType
        );
    }


}
