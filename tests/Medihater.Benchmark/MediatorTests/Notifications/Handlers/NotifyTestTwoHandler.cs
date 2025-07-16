using MediatR;

namespace Medihater.Benchmark.MediatorTests.Notifications.Handlers;
internal class NotifyTestTwoHandler : INotificationHandler<NotifyTestNotification>
{
    public Task Handle(NotifyTestNotification notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
