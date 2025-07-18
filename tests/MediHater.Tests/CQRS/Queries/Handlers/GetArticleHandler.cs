using MedihatR.Test.CQRS.DTOs;

namespace MedihatR.Test.CQRS.Queries.Handlers;
internal class GetArticleHandler : IRequestHandler<GetArticleQuery, ArticleResponse>
{
    public async Task<ArticleResponse> Handle(GetArticleQuery request, CancellationToken cancellationToken)
    {
        await Task.Delay(500);
        if (request.Id == "Throw") throw new Exception("Here is an exception");
        return new ArticleResponse()
        {
            Description = "My Description",
            Title = "The Title"
        };
    }
}
