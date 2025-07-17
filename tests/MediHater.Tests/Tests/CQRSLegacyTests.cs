using MedihatR.Test.CQRS.Commands;
using MedihatR.Test.CQRS.Commands.Handlers;
using MedihatR.Test.CQRS.DTOs;
using MedihatR.Test.CQRS.Queries;
using MedihatR.Test.CQRS.Queries.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace MedihatR.Test.Tests;
public class CQRSLegacyTests : IDisposable
{

    public void Dispose()
    {
        if (_serviceProvider is IDisposable disposable)
            disposable.Dispose();
    }
    private readonly IMedihater _medihater;
    protected readonly IServiceProvider _serviceProvider;

    public CQRSLegacyTests()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddMedihaterServices(o =>
        {
            o.Performance = Configuraions.Enums.PipelinePerformance.Reflection;
        });
        services.AddMedihaterRequestHandler<GetArticleQuery, GetArticleHandler, ArticleResponse>();
        services.AddMedihaterRequestHandler<CreateArticleCommand, CreateArticleHandler>();
        _serviceProvider = services.BuildServiceProvider();
        _medihater = _serviceProvider.GetRequiredService<IMedihater>();
    }

    [Fact]
    public async Task ShouldSuccessfully_CreateArticle_UsingNonGenericSend()
    {
        var command = new CreateArticleCommand()
        {
            Title = "",
            Description = ""
        };
        await _medihater.Send(command);

        Assert.True(CreateArticleHandler.UnitTestPassed);
    }

    [Fact]
    public async Task ShouldSuccessfully_CreateArticle_UsingGenericSend()
    {
        object command = new CreateArticleCommand()
        {
            Title = "",
            Description = ""
        };
        await _medihater.Send(command);
        Assert.True(CreateArticleHandler.UnitTestPassed);
    }
}
