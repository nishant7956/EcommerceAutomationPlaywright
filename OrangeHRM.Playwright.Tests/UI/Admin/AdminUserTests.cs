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
public sealed class AdminUserTests : BaseUiTest
{
    [Test]
    public async Task SearchAdminUser_ShouldShowAdminInResults()
    {
        await LoginAsAdminAsync();

        var adminUsers = await new AdminUsersPage(Page, Settings)
            .OpenAsync();
            
        await adminUsers.SearchByUsernameAsync("Admin");

        // Playwright natively allows us to await expectations. This handles any rendering races automatically.
        var targetRow = adminUsers.DataTable.Rows.Filter(new LocatorFilterOptions { HasText = "Admin" });
        await Expect(targetRow).Not.ToHaveCountAsync(0, new LocatorAssertionsToHaveCountOptions { Timeout = 10000 });
        
        var hasUser = await adminUsers.HasUserAsync("Admin");
        hasUser.Should().BeTrue("because the built-in Admin user should be found");
    }

    [Test]
    public async Task SaveBlankUser_ShouldShowRequiredValidationMessages()
    {
        await LoginAsAdminAsync();

        var adminUsers = await new AdminUsersPage(Page, Settings)
            .OpenAsync();
            
        await adminUsers.StartAddingUserAsync();
        await adminUsers.SaveBlankUserAsync();

        var count = await adminUsers.RequiredFieldMessageCountAsync();
        count.Should().BeGreaterThan(0, "because a blank user form should trigger validation messages");
    }
}
