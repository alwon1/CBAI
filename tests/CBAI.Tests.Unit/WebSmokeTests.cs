using Microsoft.AspNetCore.Mvc.Testing;

namespace CBAI.Tests.Unit;

[TestClass]
public sealed class WebSmokeTests
{
    [TestMethod]
    public async Task RootPage_ReturnsOk()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
