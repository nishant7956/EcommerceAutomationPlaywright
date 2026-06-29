using Microsoft.Playwright;
using OrangeHRM.Playwright.Framework.Config;

namespace OrangeHRM.Playwright.Framework.Pages;

public sealed class AddEmployeePage : BasePage
{
    private const string FirstNameInput = "input[name='firstName']";
    private const string MiddleNameInput = "input[name='middleName']";
    private const string LastNameInput = "input[name='lastName']";
    private const string SaveButton = "button[type='submit']";
    private const string PersonalDetailsHeader = "h6:has-text('Personal Details')";

    public AddEmployeePage(IPage page, TestSettings settings) : base(page, settings)
    {
    }

    public async Task<AddEmployeePage> FillNameAsync(string firstName, string middleName, string lastName)
    {
        await TypeAsync(FirstNameInput, firstName);
        await TypeAsync(MiddleNameInput, middleName);
        await TypeAsync(LastNameInput, lastName);
        return this;
    }

    public async Task<EmployeeDetailsPage> SaveAsync()
    {
        await ClickAsync(SaveButton);
        await Waiter.WaitForVisibleAsync(PersonalDetailsHeader);
        return new EmployeeDetailsPage(Page, Settings);
    }
}
