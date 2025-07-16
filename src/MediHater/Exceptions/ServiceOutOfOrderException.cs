namespace MedihatR.Exceptions;
public class ServiceOutOfOrderException : Exception
{

    public ServiceOutOfOrderException() : base($"{nameof(RegisterServicesExt.AddMedihaterServices)} should be added before any other Medihater services.")
    {
    }
}
