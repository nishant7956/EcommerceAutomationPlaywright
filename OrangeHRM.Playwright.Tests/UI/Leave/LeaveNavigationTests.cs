using FluentAssertions;
using NUnit.Framework;
using OrangeHRM.Playwright.Framework.Pages;
using OrangeHRM.Playwright.Tests.Hooks;

namespace OrangeHRM.Playwright.Tests.UI.Leave;

[TestFixture]
[Category("UI")]
[Category("Regression")]
[Category("Leave")]
public sealed class LeaveNavigationTests : BaseUiTest
{
    [Test]
    public async Task NavigateToLeave_ShouldShowLeaveListHeader()
    {
        await LoginAsAdminAsync();

        var leavePage = await new LeavePage(Page, Settings).OpenAsync();

        var isLoaded = await leavePage.IsLoadedAsync();
        isLoaded.Should().BeTrue(
            because: "clicking Leave in the sidebar should load the Leave List view");
    }

    [Test]
    public async Task LeaveList_ShouldHaveSearchFilters()
    {
        await LoginAsAdminAsync();

        var leavePage = await new LeavePage(Page, Settings).OpenAsync();

        var hasFilters = await leavePage.HasSearchFiltersAsync();
        hasFilters.Should().BeTrue(
            because: "the Leave List page must include search filter controls");
    }
}
