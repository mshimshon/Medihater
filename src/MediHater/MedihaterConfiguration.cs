namespace MedihatR;
public class MedihaterConfiguration
{
    public IEnumerable<Type> AssembliesScan { get; set; } = new List<Type>();
}
