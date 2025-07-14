using MedihatR.Engine.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace MedihatR;
public static class RegisterServicesExt
{
    private static bool _scanned;
    private static Type _requestVoidType = typeof(IRequest);
    private static Type _requestHandlerVoidType = typeof(IRequestHandler<>);

    private static Type _requestType = typeof(IRequest<>);
    private static Type _requestHandlerType = typeof(IRequestHandler<,>);

    private static Type _notificationType = typeof(INotification);
    private static Type _notificationHandlerType = typeof(INotificationHandler<>);
    public static MedihaterConfiguration Configuration { get; private set; } = new MedihaterConfiguration();
    public static IServiceCollection AddMediatCoreServices(this IServiceCollection services, Action<MedihaterConfiguration>? configure = default)
    {
        if (configure != default)
            configure(Configuration);

        if (Configuration.AssembliesScan.Count() > 0)
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
        if (
            iFace.IsGenericType && iFace.GetGenericTypeDefinition() == _requestHandlerType || iFace == _requestHandlerVoidType &&
            !services.IsServiceInterfaceRegistered(iFace)
             )
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
