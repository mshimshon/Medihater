using BenchmarkDotNet.Attributes;
using MediatR;
using MedihatR;
using Microsoft.Extensions.DependencyInjection;
namespace Medihater.Benchmark;
public class BenchmarkGeneric
{
    private readonly IMedihater _medihater;
    private readonly IMediator _mediator;
    public BenchmarkGeneric()
    {
        var services = new ServiceCollection();
        services.AddMedihaterServices(cfg =>
        {
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
    public async Task Medihater_GenericSendTest()
    {
        var ton = new MediahaterTest.Commands.CreateMyTestCommand("", "") { };
        await _medihater.Send((object)ton);
    }

    [Benchmark]
    public async Task Medihater_GenericNotificationTest()
    {
        var command = new MediahaterTest.Notifications.NotifyTestNotification("");
        await _medihater.Publish((object)command);
    }

    [Benchmark]
    public async Task Mediator_GenericSendTest()
    {
        var ton = new MediatorTests.Commands.CreateMyTestCommand("", "") { };
        await _mediator.Send((object)ton);
    }

    [Benchmark]
    public async Task Mediator_GenericNotificationTest()
    {
        var command = new MediatorTests.Notifications.NotifyTestNotification("");
        await _mediator.Publish((object)command);
    }
}
