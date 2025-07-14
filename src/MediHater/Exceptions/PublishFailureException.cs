namespace MedihatR.Exceptions;
public class PublishFailureException : Exception
{
    public Type PublisherType { get; }
    public PublishFailureException(Type publisherType, string? message) : base(message)
    {
        PublisherType = publisherType;
    }
}
