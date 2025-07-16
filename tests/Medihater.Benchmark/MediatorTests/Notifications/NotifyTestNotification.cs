using MediatR;

namespace Medihater.Benchmark.MediatorTests.Notifications;
public record NotifyTestNotification(string Message) : INotification
{
}
