using CBAI.Web.Client.Pages;
using Microsoft.FluentUI.AspNetCore.Components;

namespace CBAI.Tests.UI;

/// <summary>
/// Baseline bUnit smoke test proving the UI test harness (DI registration for FluentUI,
/// JS interop stubbing) works end-to-end, before feature-specific UI tests are written in
/// later slices.
/// </summary>
[TestClass]
public sealed class SmokeTests : Bunit.BunitContext
{
    public SmokeTests()
    {
        Services.AddFluentUIComponents();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [TestMethod]
    public void HomePage_RendersWelcomeMessage()
    {
        var cut = Render<Home>();

        Assert.AreEqual("Hello, world!", cut.Find("h1").TextContent);
    }
}
