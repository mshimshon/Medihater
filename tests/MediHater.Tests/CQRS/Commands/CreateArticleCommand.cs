namespace MedihatR.Test.CQRS.Commands;
public record CreateArticleCommand : IRequest
{
    public string Title { get; init; } = default!;
    public string Description { get; init; } = default!;
}
