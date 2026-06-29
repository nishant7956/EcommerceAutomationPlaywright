using NUnit.Framework;
using FluentAssertions;
using ECommerce.Playwright.Framework.Pages;
using ECommerce.Playwright.Tests.Hooks;

namespace ECommerce.Playwright.Tests.UI.Cart;

// ─────────────────────────────────────────────────────────────────────────────
//  CartTests
//  =========
//  Tests for the Shopping Cart/Bag page and the full Add-to-Cart flow.
// ─────────────────────────────────────────────────────────────────────────────

[TestFixture]
[Category("Cart")]
public class CartTests : BaseUiTest
{
    // ── Empty Cart ─────────────────────────────────────────────────────────────

    [Test]
    [Category("Smoke")]
    [Description("Empty cart page should display an empty state with a Continue Shopping link")]
    public async Task Empty_Cart_Shows_Empty_State_Message()
    {
        // Arrange
        var cartPage = new CartPage(Page, Settings);

        // Act
        await cartPage.NavigateAsync();

        // Assert
        (await cartPage.IsEmptyAsync()).Should().BeTrue("a fresh session should result in an empty cart");
    }

    // ── Add to Cart Flow ───────────────────────────────────────────────────────

    [Test]
    [Description("Adding a product from the storefront should increment the cart badge in the navbar")]
    public async Task Adding_Product_Updates_Cart_Badge()
    {
        // Arrange: First, create a product so there is something to add
        var productsPage = new ProductsPage(Page, Settings);
        string productName = $"Badge Test {Guid.NewGuid().ToString()[..6]}";
        await productsPage.NavigateAsync();
        await productsPage.ClickCreateProductAsync();
        await productsPage.FillProductDetailsAsync(productName, "Badge test product", "9.99", "5");
        await productsPage.SubmitProductAsync();

        // Navigate to storefront and add the product to cart
        var storefront = new StorefrontPage(Page, Settings);
        await storefront.NavigateAsync();

        // Act
        await storefront.AddProductToCartByNameAsync(productName);

        // Assert: badge should appear with count 1
        (await storefront.IsCartBadgeVisibleAsync()).Should().BeTrue("the cart badge should be visible after adding an item");
        (await storefront.GetCartBadgeTextAsync()).Should().Be("1", "the badge should show 1 item");
    }

    [Test]
    [Description("Added product should appear in the shopping cart page")]
    public async Task Added_Product_Appears_In_Cart()
    {
        // Arrange: Create a product
        var productsPage = new ProductsPage(Page, Settings);
        string productName = $"Cart Item {Guid.NewGuid().ToString()[..6]}";
        await productsPage.NavigateAsync();
        await productsPage.ClickCreateProductAsync();
        await productsPage.FillProductDetailsAsync(productName, "Cart test product", "14.99", "3");
        await productsPage.SubmitProductAsync();

        // Add to cart from storefront
        var storefront = new StorefrontPage(Page, Settings);
        await storefront.NavigateAsync();
        await storefront.AddProductToCartByNameAsync(productName);

        // Act: Navigate to cart
        var cartPage = new CartPage(Page, Settings);
        await cartPage.NavigateAsync();

        // Assert
        (await cartPage.IsItemInCartAsync(productName)).Should().BeTrue(
            "the product we added should appear in the cart");
        (await cartPage.GetCartItemCountAsync()).Should().BeGreaterThan(0, "cart should not be empty");
    }

    // ── Cart Functionality ─────────────────────────────────────────────────────

    [Test]
    [Description("Cart page should display a Secure Checkout button when items are present")]
    public async Task Cart_Shows_Checkout_Button_When_Items_Present()
    {
        // Arrange: Create and add a product
        var productsPage = new ProductsPage(Page, Settings);
        string productName = $"Checkout Btn {Guid.NewGuid().ToString()[..6]}";
        await productsPage.NavigateAsync();
        await productsPage.ClickCreateProductAsync();
        await productsPage.FillProductDetailsAsync(productName, "Checkout button test", "24.99", "10");
        await productsPage.SubmitProductAsync();

        var storefront = new StorefrontPage(Page, Settings);
        await storefront.NavigateAsync();
        await storefront.AddProductToCartByNameAsync(productName);

        // Act
        var cartPage = new CartPage(Page, Settings);
        await cartPage.NavigateAsync();

        // Assert
        (await cartPage.IsCheckoutButtonVisibleAsync()).Should().BeTrue(
            "the Secure Checkout button should be visible when the cart has items");
    }

    [Test]
    [Description("Continue Shopping link in cart should navigate back to the homepage storefront")]
    public async Task Continue_Shopping_Navigates_Back_To_Storefront()
    {
        // Arrange: Create and add a product so cart is not empty
        var productsPage = new ProductsPage(Page, Settings);
        string productName = $"Cont Shop {Guid.NewGuid().ToString()[..6]}";
        await productsPage.NavigateAsync();
        await productsPage.ClickCreateProductAsync();
        await productsPage.FillProductDetailsAsync(productName, "Continue shop test", "24.99", "10");
        await productsPage.SubmitProductAsync();

        var storefront = new StorefrontPage(Page, Settings);
        await storefront.NavigateAsync();
        await storefront.AddProductToCartByNameAsync(productName);

        var cartPage = new CartPage(Page, Settings);
        await cartPage.NavigateAsync();

        // Act
        await cartPage.ClickContinueShoppingAsync();

        // Assert
        Page.Url.Should().Be($"{Settings.BaseUrl}/",
            "Continue Shopping should take the user back to the homepage");
    }
}
