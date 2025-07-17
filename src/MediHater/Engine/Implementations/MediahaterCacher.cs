using MedihatR.Middlewares;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace MedihatR.Engine.Implementations;
delegate Task<object> ResponseInvoker(IServiceProvider serviceProvider, object request, CancellationToken ct);
delegate Task VoidInvoker(IServiceProvider serviceProvider, object request, CancellationToken ct);
delegate List<NotificationInvokerHook> NotificationInvoker(IServiceProvider sp, object notification, CancellationToken ct);
internal static class MediahaterCacher
{
    private static readonly ConcurrentDictionary<Type, Type> _notificationTypeCache = new();
    private static ResponseInvoker GetLegacyMethodOrCache(Type requestType, Type responseType)
    {

        var methodName = nameof(IRequestHandler<IRequest<object>, object>.Handle);
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
        var handleMethod = handlerType.GetMethod(methodName)!;

        return async (serviceProvider, request, ct) =>
        {
            var handler = serviceProvider.GetRequiredService(handlerType);
            var task = (Task)handleMethod.Invoke(handler, new[] { request, ct })!;
            await task.ConfigureAwait(false);

            if (responseType == typeof(void))
                return null!;

            var resultProperty = task.GetType().GetProperty("Result");
            var result = resultProperty?.GetValue(task);
            return result!;
        };
    }
    private static VoidInvoker GetLegacyVoidMethodOrCache(Type requestType)
    {
        var methodName = nameof(IRequestHandler<IRequest>.Handle);
        var handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);
        var handleMethod = handlerType.GetMethod(methodName);

        return (sp, request, ct) =>
        {
            var handler = sp.GetRequiredService(handlerType);
            var task = (Task)handleMethod!.Invoke(handler, new[] { request, ct })!;
            return task;
        };
    }
    private static NotificationInvoker GetLegacyNotificationMethodOrCache(Type notificationType)
    {

        var methodName = nameof(INotificationHandler<INotification>.Handle);
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);
        var handleMethod = handlerType.GetMethod(methodName);

        return (serviceProvider, notification, ct) =>
        {
            var services = serviceProvider.GetServices(handlerType);
            var middlwares = serviceProvider.GetServices<IPublisherMiddleware>();
            var result = services.Select(p =>
            {
                var typeHandler = p!.GetType();
                return new NotificationInvokerHook()
                {
                    Handler = () => (Task)handleMethod!.Invoke(p, new[] { notification, ct })!,
                    MiddlwareBeforePublish = middlwares
                    .Select(p => p.BeforePublish(notification, typeHandler, ct)).ToList(),
                    Cancellation = ct,
                    HandlerType = typeHandler
                };
            });
            return result.ToList();
        };
    }
    public static Type GetNotificationTypeOrCache(Type notificationType)
    {
        var methodName = nameof(INotificationHandler<INotification>.Handle);
        return _notificationTypeCache.GetOrAdd(notificationType,
            k => typeof(INotificationHandler<>).MakeGenericType(notificationType));
    }

#if NET6_0_OR_GREATER
    private static readonly ConcurrentDictionary<Type, ResponseInvoker> _responseCache = new();
    private static readonly ConcurrentDictionary<Type, VoidInvoker> _voidCache = new();
    private static readonly ConcurrentDictionary<Type, NotificationInvoker> _notificationMulticastCache = new();




    public static ResponseInvoker GetMethodOrCache(Type requestType, Type responseType)
    {
        if (RegisterServicesExt.Configuration.Performance == Configuraions.Enums.PipelinePerformance.Reflection)
            return GetLegacyMethodOrCache(requestType, responseType);
        return _responseCache.GetOrAdd(requestType, (key) => CreateResponseInvoker(requestType, responseType));
    }
    public static VoidInvoker GetVoidMethodOrCache(Type requestType)
    {
        if (RegisterServicesExt.Configuration.Performance == Configuraions.Enums.PipelinePerformance.Reflection)
            return GetLegacyVoidMethodOrCache(requestType);

        var methodName = nameof(IRequestHandler<IRequest<object>>.Handle);

        return _voidCache.GetOrAdd(requestType, key => CreateVoidInvoker(requestType, methodName, typeof(IRequestHandler<>).MakeGenericType(requestType)));
    }
    private static ResponseInvoker CreateResponseInvoker(Type requestType, Type responseType)
    {
        var methodName = nameof(IRequestHandler<IRequest<object>, object>.Handle);

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
        var handleMethod = handlerType.GetMethod(methodName, new[] { requestType, typeof(CancellationToken) })
            ?? throw new InvalidOperationException("Handle method not found");

        var boxResultMethod = typeof(MediahaterCacher)
            .GetMethod(nameof(BoxResult), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(responseType);

        var dm = new DynamicMethod(
            $"Invoke_{handlerType.Name}_Response",
            typeof(Task<object>),
            new[] { typeof(IServiceProvider), typeof(object), typeof(CancellationToken) },
            typeof(MediahaterCacher).Module,
            skipVisibility: true);

        var il = dm.GetILGenerator();
        var handlerLocal = il.DeclareLocal(handlerType);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldtoken, handlerType);
        il.Emit(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle))!);
        var getRequiredService = typeof(ServiceProviderServiceExtensions)
            .GetMethod(nameof(ServiceProviderServiceExtensions.GetRequiredService), new[] { typeof(IServiceProvider), typeof(Type) })!;

        il.Emit(OpCodes.Call, getRequiredService);
        il.Emit(OpCodes.Castclass, handlerType);
        il.Emit(OpCodes.Stloc, handlerLocal);
        il.Emit(OpCodes.Ldloc, handlerLocal);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Castclass, requestType);
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Callvirt, handleMethod);
        il.Emit(OpCodes.Call, boxResultMethod);
        il.Emit(OpCodes.Ret);

        return (ResponseInvoker)dm.CreateDelegate(typeof(ResponseInvoker));
    }
    private static async Task<object> BoxResult<T>(Task<T> task)
    {
        return (await task.ConfigureAwait(false))!;
    }
    private static VoidInvoker CreateVoidInvoker(Type requestType, string methodName, Type voidHandlerType)
    {

        var handleMethod = voidHandlerType.GetMethod(methodName, new[] { requestType, typeof(CancellationToken) })!;

        // Dynamic method signature:
        // Task Invoke(object handler, object request, CancellationToken ct)
        var dm = new DynamicMethod(
            $"Invoke_{voidHandlerType.Name}_Void",
            typeof(Task),
            new[] { typeof(IServiceProvider), typeof(object), typeof(CancellationToken) },
            typeof(MediahaterCacher).Module,
            skipVisibility: true);

        var il = dm.GetILGenerator();
        var handlerLocal = il.DeclareLocal(voidHandlerType);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldtoken, voidHandlerType);
        il.Emit(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle))!);
        var getRequiredService = typeof(ServiceProviderServiceExtensions)
            .GetMethod(nameof(ServiceProviderServiceExtensions.GetRequiredService), new[] { typeof(IServiceProvider), typeof(Type) })!;

        il.Emit(OpCodes.Call, getRequiredService);
        il.Emit(OpCodes.Castclass, voidHandlerType);
        il.Emit(OpCodes.Stloc, handlerLocal);
        il.Emit(OpCodes.Ldloc, handlerLocal);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Castclass, requestType);
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Callvirt, handleMethod);
        il.Emit(OpCodes.Ret);

        return (VoidInvoker)dm.CreateDelegate(typeof(VoidInvoker));
    }
    private static VoidInvoker CreateVoidTasksInvoker(Type requestType, string methodName, Type voidHandlerType)
    {

        var handleMethod = voidHandlerType.GetMethod(methodName, new[] { requestType, typeof(CancellationToken) })!;

        // Dynamic method signature:
        // Task Invoke(object handler, object request, CancellationToken ct)
        var dm = new DynamicMethod(
            $"Invoke_{voidHandlerType.Name}_Void",
            typeof(Task),
            new[] { typeof(IServiceProvider), typeof(object), typeof(CancellationToken) },
            typeof(MediahaterCacher).Module,
            skipVisibility: true);

        var il = dm.GetILGenerator();
        var handlerLocal = il.DeclareLocal(voidHandlerType);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldtoken, voidHandlerType);
        il.Emit(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle))!);
        var getRequiredService = typeof(ServiceProviderServiceExtensions)
            .GetMethod(nameof(ServiceProviderServiceExtensions.GetRequiredService), new[] { typeof(IServiceProvider), typeof(Type) })!;

        il.Emit(OpCodes.Call, getRequiredService);
        il.Emit(OpCodes.Castclass, voidHandlerType);
        il.Emit(OpCodes.Stloc, handlerLocal);
        il.Emit(OpCodes.Ldloc, handlerLocal);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Castclass, requestType);
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Callvirt, handleMethod);
        il.Emit(OpCodes.Ret);

        return (VoidInvoker)dm.CreateDelegate(typeof(VoidInvoker));
    }
    public static NotificationInvoker GetNotificationMethodOrCache(Type notificationType)
    {
        if (RegisterServicesExt.Configuration.Performance == Configuraions.Enums.PipelinePerformance.Reflection)
            return GetLegacyNotificationMethodOrCache(notificationType);
        return _notificationMulticastCache.GetOrAdd(notificationType, static nt =>
        {
            var method = typeof(MediahaterCacher).GetMethod(nameof(CreateNotificationInvoker))!.MakeGenericMethod(nt);

            return (NotificationInvoker)method.Invoke(null, null)!;
        });
    }
    public static NotificationInvoker CreateNotificationInvoker<TNotification>()
        where TNotification : INotification
    {
        return (sp, notificationObj, ct) =>
        {


            var handlers = sp.GetServices<INotificationHandler<TNotification>>();
            var typedNotification = (TNotification)notificationObj;
            var middlewares = sp.GetServices<IPublisherMiddleware>();
            List<NotificationInvokerHook> hooks = new();
            foreach (var handler in handlers)
            {
                var handlerType = handler.GetType();

                var task = handler.Handle(typedNotification, ct);
                var hook = new NotificationInvokerHook()
                {
                    MiddlwareBeforePublish = middlewares.Select(p => p.BeforePublish(notificationObj, handlerType, ct)).ToList(),
                    Handler = () => task,
                    Cancellation = ct,
                    HandlerType = handlerType
                };
                hooks.Add(hook);
            }

            return hooks;
        };
    }

#else
     public static ResponseInvoker GetMethodOrCache(Type requestType, Type responseType)
        => GetLegacyMethodOrCache(requestType, responseType);

    public static VoidInvoker GetVoidMethodOrCache(Type requestType)
        => GetLegacyVoidMethodOrCache(requestType);

    public static NotificationInvoker GetNotificationMethodOrCache(Type notificationType)
            => GetLegacyNotificationMethodOrCache(notificationType);
#endif
}

