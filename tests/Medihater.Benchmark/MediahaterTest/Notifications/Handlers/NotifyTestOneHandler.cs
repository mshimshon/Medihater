using MedihatR;

namespace Medihater.Benchmark.MediahaterTest.Notifications.Handlers;
internal class NotifyTestOneHandler : INotificationHandler<NotifyTestNotification>
{
    public Task Handle(NotifyTestNotification notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
