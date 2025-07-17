namespace MedihatR.Middlewares;
internal interface IRequestMiddleware
{
    Task BeforeHandler(object request, Type handlerType, CancellationToken cancellation = default);
    Task WhenHandlerCancelled(object request, Type handlerType, CancellationToken cancellation = default);
    Task WhenHandlerFailed(object request, Type handlerType, CancellationToken cancellation = default);
    Task WhenHandlerSucceed(object request, Type handlerType, CancellationToken cancellation = default);
    Task AfterHandler(object request, Type handlerType, CancellationToken cancellation = default);
}
