using MedihatR.Test.CQRS.Commands;
using MedihatR.Test.CQRS.Commands.Handlers;
using MedihatR.Test.CQRS.DTOs;
using MedihatR.Test.CQRS.Queries;
using MedihatR.Test.CQRS.Queries.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace MedihatR.Test;
public abstract class TestBase : IDisposable
{
    protected readonly IServiceProvider ServiceProvider;

    protected TestBase()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddMedihaterServices();
        services.AddMedihaterRequestHandler<GetArticleQuery, GetArticleHandler, ArticleResponse>();
        services.AddMedihaterRequestHandler<CreateArticleCommand, CreateArticleHandler>();
        ServiceProvider = services.BuildServiceProvider();
    }
    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
            disposable.Dispose();
    }
}