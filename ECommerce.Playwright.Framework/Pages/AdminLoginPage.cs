using Microsoft.Playwright;
using ECommerce.Playwright.Framework.Config;

namespace ECommerce.Playwright.Framework.Pages;

// ─────────────────────────────────────────────────────────────────────────────
//  AdminLoginPage
//  ==============
//  Page Object for the Admin Login page at /AdminAuth/Login.
//  This page uses its own full-page layout (no site navbar/footer).
// ─────────────────────────────────────────────────────────────────────────────

public class AdminLoginPage : BasePage
{
    // ── Locators ──────────────────────────────────────────────────────────────
    private ILocator UsernameInput   => Page.Locator("#admin-username");
    private ILocator PasswordInput   => Page.Locator("#admin-password");
    private ILocator LoginButton     => Page.Locator("#btn-admin-login");
    private ILocator TogglePwButton  => Page.Locator("#toggle-pw-btn");
    private ILocator ErrorAlert      => Page.Locator(".error-alert");
    private ILocator BrandName       => Page.Locator(".brand-name");
    private ILocator BackToStoreLink => Page.Locator("a[href='/']");
    private ILocator DemoHint        => Page.Locator(".demo-hint");

    public AdminLoginPage(IPage page, TestSettings settings) : base(page, settings)
    {
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    public async Task NavigateAsync()
    {
        await Page.GotoAsync($"{Settings.BaseUrl}/AdminAuth/Login");
        await WaitForLoadStateAsync();
    }

    // ── Actions ───────────────────────────────────────────────────────────────

    public async Task LoginAsync(string username, string password)
    {
        await UsernameInput.FillAsync(username);
        await PasswordInput.FillAsync(password);
        await LoginButton.ClickAsync();
        await WaitForLoadStateAsync();
    }

    public async Task TogglePasswordVisibilityAsync()
    {
        await TogglePwButton.ClickAsync();
    }

    // ── Assertions helpers ────────────────────────────────────────────────────

    public async Task<bool> IsErrorAlertVisibleAsync() => await ErrorAlert.IsVisibleAsync();

    public async Task<string> GetErrorMessageAsync() => await ErrorAlert.InnerTextAsync();

    public async Task<bool> IsBrandVisibleAsync() => await BrandName.IsVisibleAsync();

    public async Task<string> GetPasswordInputTypeAsync()
        => await PasswordInput.GetAttributeAsync("type") ?? "password";

    public async Task<bool> IsDemoHintVisibleAsync() => await DemoHint.IsVisibleAsync();

    public async Task<bool> IsOnLoginPageAsync()
        => Page.Url.Contains("/AdminAuth/Login", StringComparison.OrdinalIgnoreCase);

    public async Task<bool> IsRedirectedToAdminAsync()
        => Page.Url.Contains("/Products", StringComparison.OrdinalIgnoreCase);
}
