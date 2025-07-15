using MedihatR.Test.Notifications;
using MedihatR.Test.Notifications.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace MedihatR.Test.Tests;
public class NotificationTests : TestBase
{
    private readonly IMedihater _medihater;

    public NotificationTests()
    {
        _medihater = ServiceProvider.GetRequiredService<IMedihater>();
    }

    [Fact]
    public async Task ShouldSuccessfully_PublishCascadedNotifications()
    {
        var notification = new SpreadMeNotification("My Message");
        await _medihater.Publish(notification);
        Assert.True(SpreadListennerOneHandler.HasUnitTestPassed);
        Assert.True(SpreadListennerTwoHandler.HasUnitTestPassed);
    }
}
