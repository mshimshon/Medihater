
using MediatR;

namespace Medihater.Benchmark.MediatorTests.Commands;
public record CreateMyTestCommand(string ValueOne, string ValueTwo) : IRequest<Guid>
{
}
