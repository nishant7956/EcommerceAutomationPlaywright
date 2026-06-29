using FluentAssertions;
using NUnit.Framework;
using OrangeHRM.Playwright.Tests.Hooks;

namespace OrangeHRM.Playwright.Tests.UI.Smoke;

[TestFixture]
public class SmokeTest : BaseUiTest
{
    [Test]
    [Category("Smoke")]
    public async Task Admin_CanLogin_Successfully()
    {
        // Act: Navigate to login and log in with Admin credentials
        var dashboard = await LoginAsAdminAsync();

        // Assert: The dashboard header should be visible
        var isDashboardLoaded = await dashboard.IsLoadedAsync();
        
        isDashboardLoaded.Should().BeTrue("because logging in with valid admin credentials should navigate to the dashboard");
    }
}
