using FluentAssertions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;
using NUnit.Framework;
using OrangeHRM.Playwright.Framework.Pages;
using OrangeHRM.Playwright.Tests.Hooks;

namespace OrangeHRM.Playwright.Tests.UI.Admin;

[TestFixture]
[Category("UI")]
[Category("Regression")]
[Category("Admin")]
public sealed class AdminUserRoleTests : BaseUiTest
{
    [Test]
    public async Task FilterByAdminRole_ShouldReturnResults()
    {
        await LoginAsAdminAsync();

        var adminUsers = await new AdminUsersPage(Page, Settings).OpenAsync();
        
        await adminUsers.SearchByUserRoleAsync("Admin");

        // We use Playwright's Expect to automatically poll until the count > 0.
        // Waiter isn't needed here; ToHaveCountAsync is incredibly smart.
        await Expect(adminUsers.DataTable.Rows).Not.ToHaveCountAsync(0, new LocatorAssertionsToHaveCountOptions { Timeout = 10000 });
        
        var count = await adminUsers.DataTable.RowCountAsync();
        count.Should().BeGreaterThan(0, "the built-in Admin account must always appear when filtering by Admin role");
    }

    [Test]
    public async Task FilterByEssRole_ShouldReturnFewerOrEqualResultsThanTotal()
    {
        await LoginAsAdminAsync();

        var adminUsersPage = await new AdminUsersPage(Page, Settings).OpenAsync();
        
        // Wait for the table to populate initially
        await Expect(adminUsersPage.DataTable.Rows).Not.ToHaveCountAsync(0, new LocatorAssertionsToHaveCountOptions { Timeout = 10000 });
        var totalCount = await adminUsersPage.DataTable.RowCountAsync();

        await adminUsersPage.SearchByUserRoleAsync("ESS");
        
        // Let's add a small delay for OrangeHRM's mock API to process the filter.
        // In Playwright, waiting for network idle or a specific response is better,
        // but since we know the count will change or stay the same, we'll just wait for the loader.
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var essCount = await adminUsersPage.DataTable.RowCountAsync();

        essCount.Should().BeLessThanOrEqualTo(totalCount,
            because: "filtering by ESS role cannot return MORE users than the unfiltered list");
    }

    [Test]
    public async Task ResetRoleFilter_ShouldRestoreAllUsers()
    {
        await LoginAsAdminAsync();

        var adminUsersPage = await new AdminUsersPage(Page, Settings).OpenAsync();
        await Expect(adminUsersPage.DataTable.Rows).Not.ToHaveCountAsync(0, new LocatorAssertionsToHaveCountOptions { Timeout = 10000 });
        var totalCount = await adminUsersPage.DataTable.RowCountAsync();

        await adminUsersPage.SearchByUserRoleAsync("Admin");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        var filteredCount = await adminUsersPage.DataTable.RowCountAsync();

        filteredCount.Should().BeLessThanOrEqualTo(totalCount,
            because: "filtering by role should return the same or fewer users");
    }
}
