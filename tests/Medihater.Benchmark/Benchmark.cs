using BenchmarkDotNet.Attributes;
using MedihatR;
using Microsoft.Extensions.DependencyInjection;
namespace Medihater.Benchmark;
public class Benchmarks
{
    private IMedihater _medihater;
    public Benchmarks()
    {
        var services = new ServiceCollection();
        services.AddMedihaterServices(cfg =>
        {
            cfg.Performance = MedihatR.Configuraions.Enums.PipelinePerformance.Reflection;
            cfg.AssembliesScan = [
                typeof(Program)
            ];
        });
        services.AddMediatR(p =>
        {
            p.RegisterServicesFromAssembly(typeof(Program).Assembly);
        });
        var provider = services.BuildServiceProvider();
        _medihater = provider.GetRequiredService<IMedihater>();
    }
    [Benchmark]
    public async Task Medihater_SendTest()
    {
        var ton = new MediahaterTest.Commands.CreateMyTestCommand("", "") { };
        var t = await _medihater.Send(ton);
    }

    [Benchmark]
    public async Task Medihater_NotificationTest()
    {
        await _medihater.Publish(new MediahaterTest.Notifications.NotifyTestNotification(""));
    }

    [Benchmark]
    public async Task Mediator_SendTest()
    {
        var ton = new MediahaterTest.Commands.CreateMyTestCommand("", "") { };
        var t = await _medihater.Send(ton);
    }

    [Benchmark]
    public async Task Mediator_NotificationTest()
    {
        await _medihater.Publish(new MediahaterTest.Notifications.NotifyTestNotification(""));
    }
}
