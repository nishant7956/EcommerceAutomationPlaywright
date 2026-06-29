using NUnit.Framework;
using FluentAssertions;
using ECommerce.Playwright.Framework.Pages;
using ECommerce.Playwright.Tests.Hooks;

namespace ECommerce.Playwright.Tests.UI.Products;

public class ProductUiTests : BaseUiTest
{
    [Test]
    public async Task Can_Create_New_Product_And_Verify_In_Table()
    {
        // Arrange
        var productsPage = new ProductsPage(Page, Settings);
        string uniqueProductName = $"Test Product {Guid.NewGuid().ToString().Substring(0, 8)}";
        
        // Act
        await productsPage.NavigateAsync();
        await productsPage.ClickCreateProductAsync();
        
        await productsPage.FillProductDetailsAsync(
            name: uniqueProductName,
            description: "A great test product.",
            price: "19.99",
            stock: "100"
        );
        await productsPage.SubmitProductAsync();
        
        // Assert
        bool isProductVisible = await productsPage.IsProductInTableAsync(uniqueProductName);
        isProductVisible.Should().BeTrue("because the newly created product should appear in the catalog table");
    }
}
