using Microsoft.Playwright;
using OrangeHRM.Playwright.Framework.Components;
using OrangeHRM.Playwright.Framework.Config;

namespace OrangeHRM.Playwright.Framework.Pages;

public sealed class EmployeeListPage : BasePage
{
    private ILocator EmployeeNameInput => Page.Locator(".oxd-input-group").Filter(new() { HasText = "Employee Name" }).Locator("input");
    private ILocator EmployeeIdInput => Page.Locator(".oxd-input-group").Filter(new() { HasText = "Employee Id" }).Locator("input");
    private ILocator EmployeeListHeader => Page.Locator("h5:has-text('Employee Information')");

    public EmployeeListPage(IPage page, TestSettings settings) : base(page, settings)
    {
    }

    public async Task<EmployeeListPage> OpenAsync()
    {
        await Page.GotoAsync($"{Settings.BaseUrl}/pim/viewEmployeeList");
        
        // Wait for the URL to contain pim and wait for the header
        await Page.WaitForURLAsync("**/pim/**");
        await EmployeeListHeader.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        
        return this;
    }

    public async Task<EmployeeListPage> SearchByEmployeeNameAsync(string employeeName)
    {
        await EmployeeNameInput.FillAsync(employeeName);
        
        // In Playwright, waiting for the autocomplete dropdown is much cleaner.
        // We select the option from the listbox.
        var autocompleteOption = Page.Locator("div[role='listbox'] >> text=" + employeeName);
        await autocompleteOption.First.ClickAsync();
        
        await new SearchFilterPanel(Page).SearchAsync();
        await WaitForLoaderToVanishAsync();
        return this;
    }

    // Expose DataTable directly so tests can use Expectations
    public DataTableComponent DataTable => new DataTableComponent(Page);

    public async Task<bool> HasEmployeeAsync(string employeeName)
    {
        return await DataTable.ContainsTextAsync(employeeName);
    }

    /// <summary>Searches the employee list by exact Employee ID string.</summary>
    public async Task<EmployeeListPage> SearchByEmployeeIdAsync(string employeeId)
    {
        await EmployeeIdInput.FillAsync(employeeId);
        await new SearchFilterPanel(Page).SearchAsync();
        await WaitForLoaderToVanishAsync();
        return this;
    }

    /// <summary>Returns true when the table shows the "No Records Found" state.</summary>
    public async Task<bool> NoResultsVisibleAsync()
    {
        return await DataTable.NoResultsVisibleAsync();
    }

    /// <summary>
    /// Types a name into the Employee Name field and submits the search WITHOUT
    /// selecting an autocomplete option. Use this for negative/no-results tests.
    /// </summary>
    public async Task<EmployeeListPage> SearchByEmployeeName_NoAutocompleteAsync(string employeeName)
    {
        var input = EmployeeNameInput;
        
        // Wait for it to be interactive
        await input.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        
        // Use FillAsync to overwrite anything there
        await input.FillAsync(employeeName);
        
        // Press Enter or Blur so the React form captures the value before clicking search
        await input.BlurAsync();
        
        await new SearchFilterPanel(Page).SearchAsync();
        await WaitForLoaderToVanishAsync();
        return this;
    }

    /// <summary>Clicks the Reset button to clear all active search filters.</summary>
    public async Task<EmployeeListPage> ResetFilterAsync()
    {
        await new SearchFilterPanel(Page).ResetAsync();
        await WaitForLoaderToVanishAsync();
        return this;
    }
    
    private async Task WaitForLoaderToVanishAsync()
    {
        // OrangeHRM makes API calls when searching/resetting.
        // It's much safer to wait for network idle than trying to catch the fast-appearing loader.
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
