using MedihatR.Test.CQRS.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace MedihatR.Test.Tests;
public class CQRSTests : TestBase
{
    private readonly IMedihater _medihater;

    public CQRSTests()
    {
        _medihater = ServiceProvider.GetRequiredService<IMedihater>();
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
