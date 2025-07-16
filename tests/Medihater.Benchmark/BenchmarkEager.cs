using BenchmarkDotNet.Attributes;
using MediatR;
using MedihatR;
using Microsoft.Extensions.DependencyInjection;
namespace Medihater.Benchmark;
public class BenchmarkEager
{
    private readonly IMedihater _medihater;
    private readonly IMediator _mediator;
    public BenchmarkEager()
    {
        var services = new ServiceCollection();
        services.AddMedihaterServices(cfg =>
        {
            cfg.Performance = MedihatR.Configuraions.Enums.PipelinePerformance.DynamicMethods;
            cfg.CachingMode = MedihatR.Configuraions.Enums.PipelineCachingMode.EagerCaching;
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
    public async Task Medihater_ExplicitEagerSendTest()
    {
        var ton = new MediahaterTest.Commands.CreateMyTestCommand("", "") { };
        var t = await _medihater.Send(ton);
    }

    [Benchmark]
    public async Task Medihater_ExplicitEagerNotificationTest()
    {
        await _medihater.Publish(new MediahaterTest.Notifications.NotifyTestNotification(""));
    }

    [Benchmark]
    public async Task Mediator_ExplicitEagerSendTest()
    {
        var ton = new MediatorTests.Commands.CreateMyTestCommand("", "") { };
        await _mediator.Send<Guid>(ton);
    }

    [Benchmark]
    public async Task Mediator_ExplicitEagerNotificationTest()
    {
        await _mediator.Publish(new MediatorTests.Notifications.NotifyTestNotification(""));
    }
}
