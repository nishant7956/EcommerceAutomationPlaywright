using NUnit.Framework;
using FluentAssertions;
using ECommerce.Playwright.Framework.Pages;
using ECommerce.Playwright.Tests.Hooks;

namespace ECommerce.Playwright.Tests.UI.Storefront;

// ─────────────────────────────────────────────────────────────────────────────
//  StorefrontTests
//  ===============
//  Tests for the public-facing NEXUS Store storefront homepage.
// ─────────────────────────────────────────────────────────────────────────────

[TestFixture]
[Category("Storefront")]
[Category("Smoke")]
public class StorefrontTests : BaseUiTest
{
    // ── Layout & Rendering ─────────────────────────────────────────────────────

    [Test]
    [Description("Homepage should render all main sections: announcement bar, hero, features bar")]
    public async Task Storefront_Homepage_Renders_All_Key_Sections()
    {
        // Arrange
        var storefront = new StorefrontPage(Page, Settings);

        // Act
        await storefront.NavigateAsync();

        // Assert
        (await storefront.IsAnnouncementBarVisibleAsync()).Should().BeTrue("the announcement bar should be at the top of the page");
        (await storefront.IsHeroVisibleAsync()).Should().BeTrue("the hero section should be visible on the homepage");
        (await storefront.IsFeaturesBarVisibleAsync()).Should().BeTrue("the trust-signal features bar should be visible");
    }

    [Test]
    [Description("Hero section should display the correct brand title")]
    public async Task Storefront_Hero_Displays_Correct_Title()
    {
        // Arrange
        var storefront = new StorefrontPage(Page, Settings);
        await storefront.NavigateAsync();

        // Act
        var title = await storefront.GetHeroTitleTextAsync();

        // Assert
        title.Should().Contain("Premium", "the hero title should reference the brand's premium offering");
    }

    [Test]
    [Description("Homepage should display exactly 4 category cards")]
    public async Task Storefront_Displays_Four_Category_Cards()
    {
        // Arrange
        var storefront = new StorefrontPage(Page, Settings);

        // Act
        await storefront.NavigateAsync();

        // Assert
        (await storefront.GetCategoryCardCountAsync()).Should().Be(4, "there should be exactly 4 category cards on the homepage");
    }

    [Test]
    [Description("Footer should show 4 social media links")]
    public async Task Storefront_Footer_Has_Four_Social_Links()
    {
        // Arrange
        var storefront = new StorefrontPage(Page, Settings);
        await storefront.NavigateAsync();

        // Assert
        (await storefront.GetSocialLinkCountAsync()).Should().Be(4, "there should be 4 social links in the footer");
    }

    // ── Navigation ─────────────────────────────────────────────────────────────

    [Test]
    [Description("Clicking the admin lock icon should redirect to the admin login page")]
    public async Task Admin_Icon_Click_Redirects_To_Login()
    {
        // Arrange
        var storefront = new StorefrontPage(Page, Settings);
        await storefront.NavigateAsync();

        // Act
        await storefront.ClickAdminIconAsync();

        // Assert
        Page.Url.Should().Contain("/AdminAuth/Login",
            "clicking the admin icon should navigate to the login page for unauthenticated users");
    }

    [Test]
    [Description("Clicking the cart icon should navigate to the Shopping Bag page")]
    public async Task Cart_Icon_Click_Navigates_To_Cart()
    {
        // Arrange
        var storefront = new StorefrontPage(Page, Settings);
        await storefront.NavigateAsync();

        // Act
        await storefront.ClickCartIconAsync();

        // Assert
        Page.Url.Should().Contain("/Cart", "clicking the cart bag icon should navigate to the shopping cart");
    }
}
