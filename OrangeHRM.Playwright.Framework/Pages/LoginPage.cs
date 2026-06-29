using Microsoft.Playwright;
using OrangeHRM.Playwright.Framework.Config;

namespace OrangeHRM.Playwright.Framework.Pages;

public sealed class LoginPage : BasePage
{
    // ── Selectors ─────────────────────────────────────────────────────────────
    private const string UsernameInput = "[name='username']";
    private const string PasswordInput = "[name='password']";
    private const string LoginButton   = "button[type='submit']";
    private const string AlertMessage  = ".oxd-alert-content-text";

    public LoginPage(IPage page, TestSettings settings) : base(page, settings)
    {
    }

    public async Task<LoginPage> OpenAsync()
    {
        await NavigateToAsync("auth/login");
        
        // Wait for the username input to be visible to confirm the page has rendered
        await Waiter.WaitForVisibleAsync(UsernameInput);
        
        return this;
    }

    public async Task<DashboardPage> LoginAsAsync(string username, string password)
    {
        await TypeAsync(UsernameInput, username);
        await TypeAsync(PasswordInput, password);
        await ClickAsync(LoginButton);

        // Wait for the login attempt to settle. Two outcomes are possible:
        //   1. Valid credentials   → browser redirects to /dashboard/ URL
        //   2. Invalid credentials → browser stays on login page and shows an alert
        // Playwright lets us evaluate a JS predicate until it returns true.
        await Waiter.WaitForConditionAsync(@"() => {
            return window.location.href.includes('/dashboard/') || 
                   document.querySelector('.oxd-alert-content-text') !== null;
        }");

        return new DashboardPage(Page, Settings);
    }

    public async Task<string> ErrorMessageAsync()
    {
        return await TextOfAsync(AlertMessage);
    }

    public async Task<bool> IsLoadedAsync()
    {
        return await IsVisibleAsync(UsernameInput) && await IsVisibleAsync(LoginButton);
    }
}
