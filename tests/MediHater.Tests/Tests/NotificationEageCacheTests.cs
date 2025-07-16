using MedihatR.Test.Notifications;
using MedihatR.Test.Notifications.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace MedihatR.Test.Tests;
public class NotificationEageCacheTests : IDisposable
{
    protected readonly IServiceProvider _serviceProvider;
    public void Dispose()
    {
        if (_serviceProvider is IDisposable disposable)
            disposable.Dispose();
    }
    private readonly IMedihater _medihater;

    public NotificationEageCacheTests()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddMedihaterServices(o =>
        {
            o.CachingMode = Configuraions.Enums.PipelineCachingMode.EagerCaching;
        });
        services.AddMedihaterNotificationHandler<SpreadMeNotification, SpreadListennerOneHandler>();
        services.AddMedihaterNotificationHandler<SpreadMeNotification, SpreadListennerTwoHandler>();
        _serviceProvider = services.BuildServiceProvider();
        _medihater = _serviceProvider.GetRequiredService<IMedihater>();
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
