using BenchmarkDotNet.Attributes;
using MediatR;
using MedihatR;
using Microsoft.Extensions.DependencyInjection;
namespace Medihater.Benchmark;
public class BenchmarkLazy
{
    private readonly IMedihater _medihater;
    private readonly IMediator _mediator;
    public BenchmarkLazy()
    {
        var services = new ServiceCollection();
        services.AddMedihaterServices(cfg =>
        {
            cfg.Performance = MedihatR.Configuraions.Enums.PipelinePerformance.DynamicMethods;
            cfg.CachingMode = MedihatR.Configuraions.Enums.PipelineCachingMode.LazyCaching;
            cfg.AssembliesScan = [
                typeof(Program)
            ];
        });
        services.AddLogging();

        services.AddMediatR(p =>
        {
            p.RegisterServicesFromAssembly(typeof(Program).Assembly);
        });
        var provider = services.BuildServiceProvider();
        _medihater = provider.GetRequiredService<IMedihater>();
        _mediator = provider.GetRequiredService<IMediator>();
    }
    [Benchmark]
    public async Task Medihater_ExplicitLazySendTest()
    {
        var ton = new MediahaterTest.Commands.CreateMyTestCommand("", "") { };
        await _medihater.Send(ton);
    }

    [Benchmark]
    public async Task Medihater_ExplicitLazyNotificationTest()
    {
        await _medihater.Publish(new MediahaterTest.Notifications.NotifyTestNotification(""));
    }

    [Benchmark]
    public async Task Mediator_ExplicitLazySendTest()
    {
        var ton = new MediatorTests.Commands.CreateMyTestCommand("", "") { };
        await _mediator.Send<Guid>(ton);
    }

    [Benchmark]
    public async Task Mediator_ExplicitLazyNotificationTest()
    {
        await _mediator.Publish(new MediatorTests.Notifications.NotifyTestNotification(""));
    }
}
