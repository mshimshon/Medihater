using System.Collections.Concurrent;
using System.Reflection.Emit;

namespace MedihatR.Engine.Implementations;
delegate Task<object> ResponseInvoker(object handler, object request, CancellationToken ct);
delegate Task VoidInvoker(object handler, object request, CancellationToken ct);

static class MediahaterCacher
{
    private static readonly ConcurrentDictionary<Type, Type> _requestToHandler = new();
    private static readonly ConcurrentDictionary<Type, Type> _requestVoidToHandler = new();
    public static Type GetHandlerOrCache(Type requestType, Type responseType)
    {
        return _requestToHandler.GetOrAdd(requestType, typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType));
    }

    public static Type GetVoidHandlerOrCache(Type requestType)
    {
        return _requestVoidToHandler.GetOrAdd(requestType, typeof(IRequestHandler<>).MakeGenericType(requestType));
    }

#if NET6_0_OR_GREATER
    private static readonly ConcurrentDictionary<Type, ResponseInvoker> _responseCache = new();
    private static readonly ConcurrentDictionary<Type, VoidInvoker> _voidCache = new();


    public static ResponseInvoker GetMethodOrCache(Type handlerType)
    {
        return _responseCache.GetOrAdd(handlerType, CreateResponseInvoker);
    }

    public static VoidInvoker GetVoidMethodOrCache(Type handlerType)
    {
        return _voidCache.GetOrAdd(handlerType, CreateVoidInvoker);
    }

    private static ResponseInvoker CreateResponseInvoker(Type handlerType)
    {
        // Find IRequestHandler<TRequest, TResponse>
        var ifaces = handlerType.GetInterfaces();
        var interfaceType = ifaces.First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

        var requestType = interfaceType.GenericTypeArguments[0];
        var responseType = interfaceType.GenericTypeArguments[1];
        var methodName = nameof(IRequestHandler<IRequest<object>, object>.Handle);
        var handleMethod = interfaceType.GetMethod(methodName, new[] { requestType, typeof(CancellationToken) })!;

        // Create dynamic method with signature:
        // Task<object> Invoke(object handler, object request, CancellationToken ct)
        var dm = new DynamicMethod(
            $"Invoke_{handlerType.Name}_Response",
            typeof(Task<object>),
            new[] { typeof(object), typeof(object), typeof(CancellationToken) },
            typeof(MediahaterCacher).Module,
            skipVisibility: true);

        var il = dm.GetILGenerator();

        // Load handler argument and cast to concrete handler type
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Castclass, handlerType);

        // Load request argument and cast to concrete request type
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Castclass, requestType);

        // Load CancellationToken argument
        il.Emit(OpCodes.Ldarg_2);

        // Call handler.Handle(request, ct)
        il.Emit(OpCodes.Callvirt, handleMethod);

        // The return type is Task<TResponse> but we want Task<object>
        // So cast Task<TResponse> to Task and then to Task<object>
        if (responseType.IsValueType)
        {
            // Box response if value type (usually it’s a reference type, but just in case)
            il.Emit(OpCodes.Box, responseType);
        }

        il.Emit(OpCodes.Castclass, typeof(Task<object>));

        il.Emit(OpCodes.Ret);

        return (ResponseInvoker)dm.CreateDelegate(typeof(ResponseInvoker));
    }

    private static VoidInvoker CreateVoidInvoker(Type handlerType)
    {
        // Find IRequestHandler<TRequest>
        var interfaceType = handlerType.GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<>));

        var requestType = interfaceType.GenericTypeArguments[0];
        var methodName = nameof(IRequestHandler<IRequest<object>>.Handle);
        var handleMethod = interfaceType.GetMethod(methodName, new[] { requestType, typeof(CancellationToken) })!;

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
#else
    public static ResponseInvoker GetMethodOrCache(Type handlerType)
    {
        var interfaceType = handlerType.GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

        var responseType = interfaceType.GenericTypeArguments[1];
        var methodName = nameof(IRequestHandler<IRequest<object>, object>.Handle);
        var handleMethod = interfaceType.GetMethod(methodName)!;

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

    public static VoidInvoker GetVoidMethodOrCache(Type handlerType)
    {
        var interfaceType = handlerType.GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<>));
        var methodName = nameof(IRequestHandler<IRequest>.Handle);
        var handleMethod = interfaceType.GetMethod(methodName);

        return (handler, request, ct) =>
        {
            var task = (Task)handleMethod!.Invoke(handler, new[] { request, ct })!;
            return task;
        };
    }
#endif
}

