using MedihatR.Exceptions;
using MedihatR.Middlewares;
using Microsoft.Extensions.DependencyInjection;

namespace MedihatR.Engine.Implementations;
#pragma warning disable CS0618 // Type or member is obsolete
internal class Medihater : IMedihater
#pragma warning restore CS0618 // Type or member is obsolete
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<IPublisherMiddleware> _publisherMiddlewares;
    public Medihater(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _publisherMiddlewares = _serviceProvider.GetServices<IPublisherMiddleware>();

    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
           where TNotification : INotification
    => await Publish((object)notification, cancellationToken);
    public async Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        var notificationType = notification.GetType();
        var handlerIfaceType = MediahaterCacher.GetNotificationHandlerOrCache(notificationType);
        var services = _serviceProvider.GetServices(handlerIfaceType);
        var beforeTasks = new List<Task>();
        var tasks = new Dictionary<Type, Task>();
        foreach (var handler in services)
        {
            var handlerType = handler!.GetType();

            var beforePublish = _publisherMiddlewares.Select(p => p.BeforePublish(notification, handlerType, cancellationToken));
            beforeTasks.AddRange(beforePublish);
            tasks[handlerType] = PublishHandler(notification, handler, handlerType, cancellationToken);
        }
        await Task.WhenAll(tasks.Values);
        await Task.WhenAll(beforeTasks);

        var postPublish = tasks.Keys.SelectMany(t => _publisherMiddlewares.Select(p => p.AfterPublish(notification, t, cancellationToken)));
        await Task.WhenAll(postPublish);
    }
    private Task PublishHandler(object notification, object handlerObj, Type handlerType, CancellationToken cancellationToken = default)
    {
        try
        {
            var notificationType = notification.GetType();
            var handlerIfaceType = MediahaterCacher.GetNotificationHandlerOrCache(notificationType);
            VoidInvoker handle;
            if (RegisterServicesExt.Configuration.Performance == Configuraions.Enums.PipelinePerformance.DynamicMethods)
                handle = MediahaterCacher.GetLegacyNotificationMethodOrCache(notificationType, handlerIfaceType);
            else
                handle = MediahaterCacher.GetNotificationMethodOrCache(notificationType, handlerIfaceType);

            var task = handle(handlerObj, notification, cancellationToken);
            _ = _publisherMiddlewares.Select(p => p.WhenPublishSucceed(notification, handlerType, cancellationToken));
            return task!;
        }
        catch (Exception ex)
        {
            _ = _publisherMiddlewares.Select(p => p.WhenPublishFailed(notification, handlerType, cancellationToken));
            throw new PublishFailureException(handlerType, ex.Message);
        }
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var responseType = typeof(TResponse);
        var handlerType = MediahaterCacher.GetHandlerOrCache(requestType, responseType);
        var handler = _serviceProvider.GetRequiredService(handlerType);
        var handle = MediahaterCacher.GetMethodOrCache(requestType, responseType, handlerType);
        var response = await handle(handler, request, cancellationToken);
        return (TResponse)response;
    }
    public async Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var handlerType = MediahaterCacher.GetVoidHandlerOrCache(requestType);
        var handler = _serviceProvider.GetRequiredService(handlerType);
        VoidInvoker handle;
        if (RegisterServicesExt.Configuration.Performance == Configuraions.Enums.PipelinePerformance.DynamicMethods)
            handle = MediahaterCacher.GetVoidMethodOrCache(requestType, handlerType);
        else
            handle = MediahaterCacher.GetLegacyVoidMethodOrCache(requestType, handlerType);

        await handle(handler, request, cancellationToken);
    }
    public async Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var requestType = request.GetType();
        var irequestInterface = requestType
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));

        if (irequestInterface == null)
        {

            var handlerType = MediahaterCacher.GetVoidHandlerOrCache(requestType);
            var handler = _serviceProvider.GetRequiredService(handlerType);
            VoidInvoker handle;
            if (RegisterServicesExt.Configuration.Performance == Configuraions.Enums.PipelinePerformance.DynamicMethods)
                handle = MediahaterCacher.GetVoidMethodOrCache(requestType, handlerType);
            else
                handle = MediahaterCacher.GetLegacyVoidMethodOrCache(requestType, handlerType);

            await handle(handler, request, cancellationToken);

            return null;
        }
        else
        {
            // Get TResponse
            var responseType = irequestInterface.GetGenericArguments()[0];
            var handlerType = MediahaterCacher.GetHandlerOrCache(requestType, responseType);
            var handler = _serviceProvider.GetRequiredService(handlerType);
            ResponseInvoker handle;
            if (RegisterServicesExt.Configuration.Performance == Configuraions.Enums.PipelinePerformance.DynamicMethods)
                handle = MediahaterCacher.GetMethodOrCache(requestType, responseType, handlerType);
            else
                handle = MediahaterCacher.GetLegacyMethodOrCache(requestType, responseType, handlerType);

            return await handle(handler, request, cancellationToken);
        }
    }

}
