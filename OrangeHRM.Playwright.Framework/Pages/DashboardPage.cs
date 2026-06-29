using Microsoft.Playwright;
using OrangeHRM.Playwright.Framework.Config;

namespace OrangeHRM.Playwright.Framework.Pages;

public sealed class DashboardPage : BasePage
{
    // ── Selectors ─────────────────────────────────────────────────────────────
    // Playwright natively understands CSS and XPath.
    // 'xpath=' prefix is optional for standard XPath but good for clarity.
    private ILocator DashboardHeader => Page.GetByRole(AriaRole.Heading, new() { Name = "Dashboard" });
    private ILocator UserDropdown => Page.Locator(".oxd-userdropdown-tab");
    private ILocator LogoutLink => Page.GetByRole(AriaRole.Menuitem, new() { Name = "Logout" });

    public DashboardPage(IPage page, TestSettings settings) : base(page, settings)
    {
    }

    public async Task<bool> IsLoadedAsync()
    {
        return await IsVisibleAsync(DashboardHeader);
    }

    public async Task<LoginPage> LogoutAsync()
    {
        await UserDropdown.ClickAsync();
        await LogoutLink.ClickAsync();
        
        return new LoginPage(Page, Settings);
    }
}
