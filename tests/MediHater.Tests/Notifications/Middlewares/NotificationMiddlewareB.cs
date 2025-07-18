using MedihatR.Middlewares;
using System.Collections.Concurrent;

namespace MedihatR.Test.Notifications.Middlewares;
internal class NotificationMiddlewareB : IPublisherMiddleware
{
    public static ConcurrentStack<Type> AfterPublishStack { get; } = new();
    public static ConcurrentStack<Type> BeforePublishStack { get; } = new();
    public static ConcurrentStack<Type> WhenPublishCancelledStack { get; } = new();
    public static ConcurrentStack<Type> WhenPublishFailedStack { get; } = new();
    public static ConcurrentStack<Type> WhenPublishSucceedStack { get; } = new();
    public Task AfterPublish(object notification, Type notificationHandlerType, CancellationToken cancellation = default)
    {
        AfterPublishStack.Push(notificationHandlerType);
        return Task.CompletedTask;
    }
    public Task BeforePublish(object notification, Type notificationHandlerType, CancellationToken cancellation = default)
    {
        BeforePublishStack.Push(notificationHandlerType);
        return Task.CompletedTask;
    }
    public Task WhenPublishCancelled(object notification, Type notificationHandlerType, CancellationToken cancellation = default)
    {
        WhenPublishCancelledStack.Push(notificationHandlerType);
        return Task.CompletedTask;
    }
    public Task WhenPublishFailed(object notification, Type notificationHandlerType, CancellationToken cancellation = default)
    {
        WhenPublishFailedStack.Push(notificationHandlerType);
        return Task.CompletedTask;
    }
    public Task WhenPublishSucceed(object notification, Type notificationHandlerType, CancellationToken cancellation = default)
    {
        WhenPublishSucceedStack.Push(notificationHandlerType);
        return Task.CompletedTask;
    }
}
