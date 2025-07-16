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

    public async Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        if (notification == null)
            throw new ArgumentNullException(nameof(notification));
        var notificationType = notification.GetType();
        if (!typeof(INotification).IsAssignableFrom(notificationType))
            throw new ArgumentException($"Object does not implement INotification: {notificationType}");
        var genericPublishMethod = typeof(Medihater)
            .GetMethod(nameof(Publish), new[] { notificationType, typeof(CancellationToken) });
        if (genericPublishMethod == null || !genericPublishMethod.IsGenericMethodDefinition)
        {
            genericPublishMethod = typeof(Medihater)
                .GetMethods()
                .First(m => m.Name == nameof(Publish)
                         && m.IsGenericMethodDefinition
                         && m.GetParameters().Length == 2);
        }
        var method = genericPublishMethod.MakeGenericMethod(notificationType);
        var task = (Task)method.Invoke(this, new object[] { notification, cancellationToken })!;
        await task.ConfigureAwait(false);
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
        var handle = MediahaterCacher.GetVoidMethodOrCache(requestType, handlerType);
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
            if (!typeof(IRequest).IsAssignableFrom(requestType))
                throw new ArgumentException("Request does not implement IRequest or IRequest<TResponse>");
            var sendVoidMethod = typeof(Medihater)
                .GetMethod(nameof(Send), new[] { typeof(IRequest), typeof(CancellationToken) });

            var handlerTask = (Task)sendVoidMethod!.Invoke(this, new object[] { request, cancellationToken })!;
            await handlerTask.ConfigureAwait(false);
            return null;
        }
        else
        {
            // Get TResponse
            var responseType = irequestInterface.GetGenericArguments()[0];
            var handlerType = MediahaterCacher.GetHandlerOrCache(requestType, responseType);
            var handler = _serviceProvider.GetRequiredService(handlerType);
            var handle = MediahaterCacher.GetMethodOrCache(requestType, responseType, handlerType);
            return await handle(handler, request, cancellationToken);
        }
    }

    private Task PublishHandler<TNotification>(TNotification notification, object handlerObj, Type handlerType, CancellationToken cancellationToken = default)
     where TNotification : INotification
    {
        try
        {
            var notificationType = notification.GetType();
            var handlerIfaceType = MediahaterCacher.GetNotificationHandlerOrCache(notificationType);

            var handle = MediahaterCacher.GetNotificationMethodOrCache(notificationType, handlerIfaceType);


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

}
