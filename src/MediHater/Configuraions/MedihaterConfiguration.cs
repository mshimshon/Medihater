using MedihatR.Configuraions.Enums;

namespace MedihatR.Configuraions;
public class MedihaterConfiguration
{
    public IEnumerable<Type> AssembliesScan { get; set; } = new List<Type>();
    public PipelineNotificationFireMode NotificationFireMode { get; set; } = PipelineNotificationFireMode.FireAndForget;
    public PipelinePerformance Performance { get; set; } = PipelinePerformance.DynamicMethods;
    public PipelineCachingMode CachingMode { get; set; } = PipelineCachingMode.LazyCaching;
    public PipelineNotificationMiddleware NotificationMiddleware { get; set; } = PipelineNotificationMiddleware.TriggerNotificationHandler_Then_AwaitForBoth;
    public PipelineMiddlewareWaitMode MiddlewareAwaitMode { get; set; } = PipelineMiddlewareWaitMode.OnlyAwaitOrderedExecuted;

}
