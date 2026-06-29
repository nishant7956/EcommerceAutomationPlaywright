using Microsoft.Playwright;
using OrangeHRM.Playwright.Framework.Config;

namespace OrangeHRM.Playwright.Framework.Pages;

public sealed class EmployeeDetailsPage : BasePage
{
    private ILocator PersonalDetailsHeader => Page.Locator("h6:has-text('Personal Details')");
    private ILocator NicknameInput => Page.Locator(".oxd-input-group").Filter(new() { HasText = "Nickname" }).Locator("input");
    private ILocator SaveButton => Page.Locator("button[type='submit']").First;

    public EmployeeDetailsPage(IPage page, TestSettings settings) : base(page, settings)
    {
    }

    public async Task<bool> IsLoadedAsync()
    {
        return await IsVisibleAsync(PersonalDetailsHeader);
    }

    public async Task<EmployeeDetailsPage> UpdateNicknameAsync(string nickname)
    {
        await NicknameInput.FillAsync(nickname);
        await SaveButton.ClickAsync();
        return this;
    }
}
