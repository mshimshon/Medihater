namespace MedihatR.Middlewares;
public interface IRequestMiddleware
{
    Task BeforeHandler(object request, Type handlerType, CancellationToken cancellation = default);
    Task WhenHandlerCancelled(object request, Type handlerType, CancellationToken cancellation = default);
    Task WhenPublishFailed(object request, Type handlerType, CancellationToken cancellation = default);
    Task WhenPublishSucceed(object request, Type handlerType, CancellationToken cancellation = default);
    Task AfterPublish(object request, Type handlerType, CancellationToken cancellation = default);
}
