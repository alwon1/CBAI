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
// needs to be replaced with playwright test
    [TestMethod]
    public async Task AdminUsersPage_DoesNotReturnNotFoundPage()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/admin/users");
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreNotEqual(HttpStatusCode.NotFound, response.StatusCode);
        Assert.IsFalse(content.Contains("Sorry, the content you are looking for does not exist.", StringComparison.Ordinal));
    }
}
