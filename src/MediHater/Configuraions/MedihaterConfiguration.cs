using MedihatR.Configuraions.Enums;

namespace MedihatR.Configuraions;
public class MedihaterConfiguration
{
    public IEnumerable<Type> AssembliesScan { get; set; } = new List<Type>();
    public PipelinePerformance Performance { get; set; } = PipelinePerformance.DynamicMethods;
    public PipelineCachingMode CachingMode { get; set; } = PipelineCachingMode.LazyCaching;
}
