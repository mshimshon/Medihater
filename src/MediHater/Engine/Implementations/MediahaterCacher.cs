using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace MedihatR.Engine.Implementations;
delegate Task<object> ResponseInvoker(object handler, object request, CancellationToken ct);
delegate Task VoidInvoker(object handler, object request, CancellationToken ct);

static class MediahaterCacher
{
    private static readonly ConcurrentDictionary<Type, Type> _requestToHandler = new();
    private static readonly ConcurrentDictionary<Type, Type> _requestVoidToHandler = new();
    private static readonly ConcurrentDictionary<Type, Type> _notificationToHandler = new();
    private static readonly ConcurrentDictionary<Type, VoidInvoker> _notificationCache = new();
    public static Type GetHandlerOrCache(Type requestType, Type responseType)
    {
        return _requestToHandler.GetOrAdd(requestType, typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType));
    }

    public static Type GetVoidHandlerOrCache(Type requestType)
    {
        return _requestVoidToHandler.GetOrAdd(requestType, typeof(IRequestHandler<>).MakeGenericType(requestType));
    }

    public static Type GetNotificationHandlerOrCache(Type notificationType)
    {
        return _notificationToHandler.GetOrAdd(notificationType, typeof(INotificationHandler<>).MakeGenericType(notificationType));
    }
    public static ResponseInvoker GetLegacyMethodOrCache(Type requestType, Type responseType, Type handlerType)
    {

        var methodName = nameof(IRequestHandler<IRequest<object>, object>.Handle);
        var handleMethod = handlerType.GetMethod(methodName)!;

        return async (handler, request, ct) =>
        {
            var task = (Task)handleMethod.Invoke(handler, new[] { request, ct })!;
            await task.ConfigureAwait(false);

            if (responseType == typeof(void))
                return null!;

            var resultProperty = task.GetType().GetProperty("Result");
            var result = resultProperty?.GetValue(task);
            return result!;
        };
    }

    public static VoidInvoker GetLegacyVoidMethodOrCache(Type requestType, Type handlerType)
    {
        var methodName = nameof(IRequestHandler<IRequest>.Handle);
        var handleMethod = handlerType.GetMethod(methodName);

        return (handler, request, ct) =>
        {
            var task = (Task)handleMethod!.Invoke(handler, new[] { request, ct })!;
            return task;
        };
    }
    public static VoidInvoker GetLegacyNotificationMethodOrCache(Type notificationType, Type handlerType)
    {

        var methodName = nameof(INotificationHandler<INotification>.Handle);
        var handleMethod = handlerType.GetMethod(methodName);

        return (handler, request, ct) =>
        {
            var task = (Task)handleMethod!.Invoke(handler, new[] { request, ct })!;
            return task;
        };
    }
#if NET6_0_OR_GREATER
    private static readonly ConcurrentDictionary<Type, ResponseInvoker> _responseCache = new();
    private static readonly ConcurrentDictionary<Type, VoidInvoker> _voidCache = new();


    public static ResponseInvoker GetMethodOrCache(Type requestType, Type responseType, Type handlerType)
    {
        return _responseCache.GetOrAdd(handlerType, CreateResponseInvoker(requestType, responseType, handlerType));
    }

    public static VoidInvoker GetVoidMethodOrCache(Type requestType, Type handlerType)
    {
        var methodName = nameof(IRequestHandler<IRequest<object>>.Handle);

        return _voidCache.GetOrAdd(handlerType, CreateVoidInvoker(requestType, handlerType, methodName));
    }
    private static ResponseInvoker CreateResponseInvoker(Type requestType, Type responseType, Type handlerType)
    {
        var methodName = nameof(IRequestHandler<IRequest<object>, object>.Handle);
        var handleMethod = handlerType.GetMethod(methodName, new[] { requestType, typeof(CancellationToken) })
            ?? throw new InvalidOperationException("Handle method not found");

        var boxResultMethod = typeof(MediahaterCacher)
            .GetMethod(nameof(BoxResult), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(responseType);

        var dm = new DynamicMethod(
            $"Invoke_{handlerType.Name}_Response",
            typeof(Task<object>),
            new[] { typeof(object), typeof(object), typeof(CancellationToken) },
            typeof(MediahaterCacher).Module,
            skipVisibility: true);

        var il = dm.GetILGenerator();


        // Load handler argument and cast
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Castclass, handlerType);

        // Load request argument and cast
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Castclass, requestType);

        // Load cancellation token
        il.Emit(OpCodes.Ldarg_2);

        // Call handler.Handle(...) => Task<TResponse>
        il.Emit(OpCodes.Callvirt, handleMethod);

        // Call BoxResult<TResponse>(Task<TResponse>) => Task<object>
        il.Emit(OpCodes.Call, boxResultMethod);

        // Return Task<object>
        il.Emit(OpCodes.Ret);

        return (ResponseInvoker)dm.CreateDelegate(typeof(ResponseInvoker));
    }
    private static async Task<object> BoxResult<T>(Task<T> task)
    {
        return (await task.ConfigureAwait(false))!;
    }
    private static VoidInvoker CreateVoidInvoker(Type requestType, Type handlerType, string methodName)
    {
        var handleMethod = handlerType.GetMethod(methodName, new[] { requestType, typeof(CancellationToken) })!;

        // Dynamic method signature:
        // Task Invoke(object handler, object request, CancellationToken ct)
        var dm = new DynamicMethod(
            $"Invoke_{handlerType.Name}_Void",
            typeof(Task),
            new[] { typeof(object), typeof(object), typeof(CancellationToken) },
            typeof(MediahaterCacher).Module,
            skipVisibility: true);

        var il = dm.GetILGenerator();

        // Cast handler
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Castclass, handlerType);

        // Cast request
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Castclass, requestType);

        // Load cancellation token
        il.Emit(OpCodes.Ldarg_2);

        // Call handler.Handle(request, ct)
        il.Emit(OpCodes.Callvirt, handleMethod);

        il.Emit(OpCodes.Ret);

        return (VoidInvoker)dm.CreateDelegate(typeof(VoidInvoker));
    }
    public static VoidInvoker GetNotificationMethodOrCache(Type notificationType, Type handlerType)
    {
        var methodName = nameof(INotificationHandler<INotification>.Handle);

        return _notificationCache.GetOrAdd(handlerType, CreateVoidInvoker(notificationType, handlerType, methodName));
    }


#else
     public static ResponseInvoker GetMethodOrCache(Type requestType, Type responseType, Type handlerType)
        => GetLegacyMethodOrCache(requestType, responseType, handlerType);

    public static VoidInvoker GetVoidMethodOrCache(Type requestType, Type handlerType)
        => GetLegacyVoidMethodOrCache(requestType, handlerType);

    public static VoidInvoker GetNotificationMethodOrCache(Type notificationType, Type handlerType)
            => GetLegacyNotificationMethodOrCache(notificationType, handlerType);

#endif
}

