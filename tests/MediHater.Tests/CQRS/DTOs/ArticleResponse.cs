namespace MedihatR.Test.CQRS.DTOs;
public record ArticleResponse
{
    public string Title { get; init; } = default!;
    public string Description { get; init; } = default!;
}
