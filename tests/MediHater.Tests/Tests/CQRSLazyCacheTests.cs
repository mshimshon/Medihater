using MedihatR.Test.CQRS.Commands;
using MedihatR.Test.CQRS.Commands.Handlers;
using MedihatR.Test.CQRS.DTOs;
using MedihatR.Test.CQRS.Queries;
using MedihatR.Test.CQRS.Queries.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace MedihatR.Test.Tests;
public class CQRSLazyCacheTests : IDisposable
{

    public void Dispose()
    {
        if (_serviceProvider is IDisposable disposable)
            disposable.Dispose();
    }
    private readonly IMedihater _medihater;
    protected readonly IServiceProvider _serviceProvider;

    public CQRSLazyCacheTests()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddMedihaterServices(o =>
        {
            o.CachingMode = Configuraions.Enums.PipelineCachingMode.LazyCaching;
            o.MiddlewareAwaitMode = Configuraions.Enums.PipelineMiddlewareWaitMode.NeverAwaitMiddlewares;
            o.NotificationMiddleware = Configuraions.Enums.PipelineNotificationMiddleware.TriggerNotificationHandler_Then_AwaitForBoth;
        });
        services.AddMedihaterRequestHandler<GetArticleQuery, GetArticleHandler, ArticleResponse>();
        services.AddMedihaterRequestHandler<CreateArticleCommand, CreateArticleHandler>();
        _serviceProvider = services.BuildServiceProvider();
        _medihater = _serviceProvider.GetRequiredService<IMedihater>();
    }

    [Fact]
    public async Task ShouldSuccessfully_GetArticle_UsingNonGenericSend()
    {
        var response = await _medihater.Send(new GetArticleQuery("dsad"));
        Assert.NotNull(response);
    }

    [Fact]
    public async Task ShouldSuccessfully_GetArticle_UsingGenericSend()
    {
        object query = new GetArticleQuery("dsad");
        var response = await _medihater.Send(query);
        Assert.True(response!.GetType() == response.GetType());
    }
}
