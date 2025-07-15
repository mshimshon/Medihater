namespace MedihatR.Test.Notifications.Handlers;
internal class SpreadListennerTwoHandler : INotificationHandler<SpreadMeNotification>
{
    public static bool HasUnitTestPassed = false;
    public async Task Handle(SpreadMeNotification notification, CancellationToken cancellationToken)
    {
        await Task.Delay(500);
        HasUnitTestPassed = true;
    }
}
