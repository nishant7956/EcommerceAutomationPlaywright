using Microsoft.Playwright;
using OrangeHRM.Playwright.Framework.Components;
using OrangeHRM.Playwright.Framework.Config;

namespace OrangeHRM.Playwright.Framework.Pages;

public sealed class PimPage : BasePage
{
    private const string AddEmployeeTab = "a:has-text('Add Employee')";
    private const string EmployeeListHeader = "h5:has-text('Employee Information')";

    public PimPage(IPage page, TestSettings settings) : base(page, settings)
    {
    }

    public async Task<PimPage> OpenAsync()
    {
        await new SidebarMenu(Page).OpenModuleAsync("PIM");
        await Waiter.WaitForVisibleAsync(EmployeeListHeader);
        return this;
    }

    public async Task<AddEmployeePage> GoToAddEmployeeAsync()
    {
        await ClickAsync(AddEmployeeTab);
        return new AddEmployeePage(Page, Settings);
    }
}
