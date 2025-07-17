namespace MedihatR.Configuraions.Enums;
public enum PipelineMiddlewareWaitMode
{
    NeverAwaitMiddlewares,
    OnlyAwaitOrderedExecuted,
    AwaitAllMiddlewares
}
