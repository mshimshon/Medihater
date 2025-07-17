namespace MedihatR.Engine.Implementations;
internal class NotificationInvokerHook
{
    public CancellationToken Cancellation { get; set; } = default!;
    public Type HandlerType { get; set; } = default!;
    public Func<Task> Handler { get; set; } = default!;
    public IReadOnlyCollection<Task> MiddlwareBeforePublish { get; set; } = new List<Task>();
}

