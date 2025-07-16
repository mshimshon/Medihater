using MedihatR;

namespace Medihater.Benchmark.MediahaterTest.Notifications.Handlers;
internal class NotifyTestTwoHandler : INotificationHandler<NotifyTestNotification>
{
    public Task Handle(NotifyTestNotification notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
