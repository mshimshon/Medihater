using MedihatR.Test.CQRS.Commands;
using MedihatR.Test.CQRS.Commands.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace MedihatR.Test.Tests;
public class CQRSLegacyTests : TestBase
{
    private readonly IMedihater _medihater;

    public CQRSLegacyTests()
    {
        _medihater = ServiceProvider.GetRequiredService<IMedihater>();
    }
    [Fact]
    public async Task ShouldSuccessfully_CreateArticle_UsingNonGenericSend()
    {
        await _medihater.Send(new CreateArticleCommand()
        {
            Title = "",
            Description = ""
        });

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
