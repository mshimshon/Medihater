using MedihatR;

namespace Medihater.Benchmark.MediahaterTest.Commands.Handlers;
internal class CreateMyTestHandler : IRequestHandler<CreateMyTestCommand, Guid>
{
    public Task<Guid> Handle(CreateMyTestCommand request, CancellationToken cancellationToken)
        => Task.FromResult(Guid.Empty);
}
