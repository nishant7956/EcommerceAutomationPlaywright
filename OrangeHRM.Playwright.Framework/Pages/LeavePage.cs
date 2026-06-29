using Microsoft.Playwright;
using OrangeHRM.Playwright.Framework.Components;
using OrangeHRM.Playwright.Framework.Config;

namespace OrangeHRM.Playwright.Framework.Pages;

public sealed class LeavePage : BasePage
{
    private const string LeaveListHeader = "h5:has-text('Leave List')";

    public LeavePage(IPage page, TestSettings settings) : base(page, settings)
    {
    }

    public async Task<LeavePage> OpenAsync()
    {
        await new SidebarMenu(Page).OpenModuleAsync("Leave");
        await Waiter.WaitForVisibleAsync(LeaveListHeader);
        return this;
    }

    public async Task<bool> HasSearchFiltersAsync()
    {
        return await new SearchFilterPanel(Page).HasSearchButtonAsync();
    }

    /// <summary>Returns true when the Leave List page header is visible.</summary>
    public async Task<bool> IsLoadedAsync()
    {
        return await IsVisibleAsync(LeaveListHeader);
    }
}
