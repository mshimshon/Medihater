namespace MedihatR;
#pragma warning disable S2326 // Unused type parameters should be removed
public interface IRequest<out TResponse> : IRequest { }
#pragma warning restore S2326 // Unused type parameters should be removed
public interface IRequest { }