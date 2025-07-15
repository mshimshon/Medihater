using MedihatR.Test.CQRS.DTOs;

namespace MedihatR.Test.CQRS.Queries;
public record GetArticleQuery(string Id) : IRequest<ArticleResponse>
{

}
