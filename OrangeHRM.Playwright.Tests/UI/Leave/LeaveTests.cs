using FluentAssertions;
using NUnit.Framework;
using OrangeHRM.Playwright.Framework.Pages;
using OrangeHRM.Playwright.Tests.Hooks;

namespace OrangeHRM.Playwright.Tests.UI.Leave;

[TestFixture]
[Category("UI")]
[Category("Regression")]
[Category("Leave")]
public sealed class LeaveTests : BaseUiTest
{
    [Test]
    public async Task LeaveList_ShouldExposeSearchFilters()
    {
        await LoginAsAdminAsync();

        var leavePage = await new LeavePage(Page, Settings).OpenAsync();

        var hasFilters = await leavePage.HasSearchFiltersAsync();
        hasFilters.Should().BeTrue();
    }
}
