using MedihatR;

namespace Medihater.Benchmark.MediahaterTest.Commands;
public record CreateMyTestCommand(string ValueOne, string ValueTwo) : IRequest<Guid>
{
}
