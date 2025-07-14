namespace MedihatR.Middlewares;
public interface IPublisherMiddleware
{
    Task BeforePublish(object notification, Type notificationHandlerType, CancellationToken cancellation = default);
    Task WhenPublishFailed(object notification, Type notificationHandlerType, CancellationToken cancellation = default);
    Task WhenPublishSucceed(object notification, Type notificationHandlerType, CancellationToken cancellation = default);
    Task AfterPublish(object notification, Type notificationHandlerType, CancellationToken cancellation = default);
}
