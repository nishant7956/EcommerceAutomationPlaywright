using Microsoft.Playwright;
using OrangeHRM.Playwright.Framework.Components;
using OrangeHRM.Playwright.Framework.Config;

namespace OrangeHRM.Playwright.Framework.Pages;

public sealed class AdminUsersPage : BasePage
{
    // Playwright Selectors
    private ILocator SystemUsersHeader => Page.Locator("h5:has-text('System Users')");
    private ILocator UsernameInput => Page.Locator(".oxd-input-group").Filter(new() { HasText = "Username" }).Locator("input");
    private ILocator UserRoleDropdown => Page.Locator(".oxd-input-group").Filter(new() { HasText = "User Role" }).Locator(".oxd-select-text-input");
    private ILocator AddButton => Page.Locator("button:has-text('Add')");
    private ILocator SaveButton => Page.Locator("button[type='submit']");
    private ILocator RequiredMessages => Page.Locator("span:has-text('Required')");

    public AdminUsersPage(IPage page, TestSettings settings) : base(page, settings)
    {
    }

    public async Task<AdminUsersPage> OpenAsync()
    {
        await new SidebarMenu(Page).OpenModuleAsync("Admin");
        await SystemUsersHeader.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return this;
    }

    public async Task<AdminUsersPage> SearchByUsernameAsync(string username)
    {
        await UsernameInput.FillAsync(username);
        await new SearchFilterPanel(Page).SearchAsync();
        
        var loader = Page.Locator(".oxd-form-loader");
        if (await loader.IsVisibleAsync())
        {
            await loader.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });
        }
        
        return this;
    }

    /// <summary>
    /// Filters System Users by user role (e.g., "Admin" or "ESS").
    /// </summary>
    public async Task<AdminUsersPage> SearchByUserRoleAsync(string roleName)
    {
        await UserRoleDropdown.ClickAsync();
        
        // In OrangeHRM, selecting from a dropdown opens a listbox.
        // We can just click the option with the text we want.
        await Page.Locator($"div[role='listbox'] >> text={roleName}").ClickAsync();
        
        await new SearchFilterPanel(Page).SearchAsync();
        
        // Let's add a small wait to ensure the network request for search has at least
        // settled. We can wait for the table rows to re-render.
        // Wait for the loading spinner to disappear (a common pattern in OrangeHRM)
        var loader = Page.Locator(".oxd-form-loader");
        if (await loader.IsVisibleAsync())
        {
            await loader.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });
        }
        
        return this;
    }

    // Expose the DataTable component so tests can run assertions on it
    public DataTableComponent DataTable => new DataTableComponent(Page);

    public async Task<bool> HasUserAsync(string username)
    {
        return await DataTable.ContainsTextAsync(username);
    }

    public async Task<AdminUsersPage> StartAddingUserAsync()
    {
        await AddButton.ClickAsync();
        return this;
    }

    public async Task<AdminUsersPage> SaveBlankUserAsync()
    {
        await SaveButton.ClickAsync();
        return this;
    }

    public async Task<int> RequiredFieldMessageCountAsync()
    {
        // Give validation messages a tiny bit of time to appear
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return await RequiredMessages.CountAsync();
    }
}
