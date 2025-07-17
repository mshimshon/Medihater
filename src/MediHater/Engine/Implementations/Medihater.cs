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
        NotificationInvoker notificationHandler = MediahaterCacher.GetNotificationMethodOrCache(notificationType);
        List<NotificationInvokerHook> notificationTasks = notificationHandler(_serviceProvider, notification, cancellationToken);

        await Task.WhenAll(notificationTasks.SelectMany(p => p.MiddlwareBeforePublish));
        var notificationTaskMapper = notificationTasks.ToDictionary(p => p.Handler(), p => p);
        await Task.WhenAll(notificationTaskMapper.Keys);

        foreach (var item in notificationTaskMapper)
        {
            if (item.Key.IsCanceled)
                _ = _publisherMiddlewares.Select(p => p.WhenPublishCancelled(notification, item.Value.HandlerType, cancellationToken));
            else if (item.Key.IsFaulted)
                _ = _publisherMiddlewares.Select(p => p.WhenPublishFailed(notification, item.Value.HandlerType, cancellationToken));
            else
                _ = _publisherMiddlewares.Select(p => p.WhenPublishSucceed(notification, item.Value.HandlerType, cancellationToken));

            _ = _publisherMiddlewares.Select(p => p.AfterPublish(notification, item.Value.HandlerType, cancellationToken));
        }
    }


    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var responseType = typeof(TResponse);
        var handle = MediahaterCacher.GetMethodOrCache(requestType, responseType);
        var response = await handle(_serviceProvider, request, cancellationToken);
        return (TResponse)response;
    }
    public async Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        VoidInvoker handle = MediahaterCacher.GetVoidMethodOrCache(requestType);

        await handle(_serviceProvider, request, cancellationToken);
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

            VoidInvoker handle = MediahaterCacher.GetVoidMethodOrCache(requestType);

            await handle(_serviceProvider, request, cancellationToken);

            return null;
        }
        else
        {
            // Get TResponse
            var responseType = irequestInterface.GetGenericArguments()[0];
            ResponseInvoker handle = MediahaterCacher.GetMethodOrCache(requestType, responseType);

            return await handle(_serviceProvider, request, cancellationToken);
        }
    }

}
