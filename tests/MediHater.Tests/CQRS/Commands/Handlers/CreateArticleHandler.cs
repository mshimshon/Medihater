namespace MedihatR.Test.CQRS.Commands.Handlers;
internal class CreateArticleHandler : IRequestHandler<CreateArticleCommand>
{
    public static bool UnitTestPassed = false;
    public async Task Handle(CreateArticleCommand request, CancellationToken cancellationToken)
    {
        await Task.Delay(100);
        UnitTestPassed = true;
    }
}
