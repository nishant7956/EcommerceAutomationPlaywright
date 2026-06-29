using NUnit.Framework;
using FluentAssertions;
using ECommerce.Playwright.Framework.Pages;
using ECommerce.Playwright.Tests.Hooks;

namespace ECommerce.Playwright.Tests.UI.Auth;

// ─────────────────────────────────────────────────────────────────────────────
//  AdminLoginTests
//  ===============
//  Tests for the admin login page and session-based authentication.
// ─────────────────────────────────────────────────────────────────────────────

[TestFixture]
[Category("Auth")]
[Category("Smoke")]
public class AdminLoginTests : BaseUiTest
{
    // ── Happy Path ─────────────────────────────────────────────────────────────

    [Test]
    [Description("Admin login page should render all key UI elements")]
    public async Task Admin_Login_Page_Renders_Correctly()
    {
        // Arrange
        var loginPage = new AdminLoginPage(Page, Settings);

        // Act
        await loginPage.NavigateAsync();

        // Assert
        (await loginPage.IsBrandVisibleAsync()).Should().BeTrue("the NEXUS brand should be visible on the login page");
        (await loginPage.IsDemoHintVisibleAsync()).Should().BeTrue("demo credentials hint should be visible");
        (await loginPage.IsOnLoginPageAsync()).Should().BeTrue("we should still be on the login page");
    }

    [Test]
    [Description("Valid admin credentials should redirect to the Admin Products portal")]
    public async Task Admin_Can_Login_With_Valid_Credentials()
    {
        // Arrange
        var loginPage = new AdminLoginPage(Page, Settings);
        await loginPage.NavigateAsync();

        // Act
        await loginPage.LoginAsync("admin", "nexus2026");

        // Assert
        (await loginPage.IsRedirectedToAdminAsync()).Should().BeTrue(
            "because valid credentials should redirect to the Products admin page");
    }

    // ── Error Handling ─────────────────────────────────────────────────────────

    [Test]
    [Description("Invalid credentials should show an error message and stay on login page")]
    public async Task Admin_Login_Fails_With_Invalid_Password()
    {
        // Arrange
        var loginPage = new AdminLoginPage(Page, Settings);
        await loginPage.NavigateAsync();

        // Act
        await loginPage.LoginAsync("admin", "wrongpassword123");

        // Assert
        (await loginPage.IsOnLoginPageAsync()).Should().BeTrue("user should remain on the login page after failed login");
        (await loginPage.IsErrorAlertVisibleAsync()).Should().BeTrue("an error alert should be displayed for wrong credentials");
    }

    [Test]
    [Description("Invalid username should show an error and keep user on login page")]
    public async Task Admin_Login_Fails_With_Invalid_Username()
    {
        // Arrange
        var loginPage = new AdminLoginPage(Page, Settings);
        await loginPage.NavigateAsync();

        // Act
        await loginPage.LoginAsync("notanadmin", "nexus2026");

        // Assert
        (await loginPage.IsOnLoginPageAsync()).Should().BeTrue();
        (await loginPage.IsErrorAlertVisibleAsync()).Should().BeTrue();
    }

    // ── Access Control ─────────────────────────────────────────────────────────

    [Test]
    [Description("Unauthenticated user navigating to /Products should be redirected to login")]
    public async Task Unauthenticated_Access_To_Products_Redirects_To_Login()
    {
        // Arrange & Act
        await Page.GotoAsync($"{Settings.BaseUrl}/Products");
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.DOMContentLoaded);

        // Assert
        Page.Url.Should().Contain("/AdminAuth/Login",
            "because unauthenticated users must be redirected to the admin login page");
    }

    // ── UX Features ────────────────────────────────────────────────────────────

    [Test]
    [Description("Toggle password button should switch input type from password to text")]
    public async Task Password_Toggle_Shows_And_Hides_Password()
    {
        // Arrange
        var loginPage = new AdminLoginPage(Page, Settings);
        await loginPage.NavigateAsync();

        // Assert initial state
        (await loginPage.GetPasswordInputTypeAsync()).Should().Be("password", "password is hidden by default");

        // Act: toggle visibility
        await loginPage.TogglePasswordVisibilityAsync();

        // Assert toggled state
        (await loginPage.GetPasswordInputTypeAsync()).Should().Be("text", "after toggle, password should be visible");

        // Act: toggle back
        await loginPage.TogglePasswordVisibilityAsync();

        // Assert restored state
        (await loginPage.GetPasswordInputTypeAsync()).Should().Be("password", "after second toggle, password should be hidden again");
    }
}
