namespace MedihatR.Engine.Implementations;
internal class RequestInvokerHook
{
    public CancellationToken Cancellation { get; set; } = default!;
    public Type HandlerType { get; set; } = default!;
    public Func<Task<object?>> Handler { get; set; } = default!;

}
