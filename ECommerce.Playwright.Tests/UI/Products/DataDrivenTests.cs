using NUnit.Framework;
using ECommerce.Playwright.Tests.Hooks;
using ECommerce.Playwright.Framework.Utils;

namespace ECommerce.Playwright.Tests.UI.Products;

public class ProductData
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

[TestFixture]
public class DataDrivenTests : BaseUiTest
{
    public static IEnumerable<TestCaseData> ProductTestData()
    {
        return TestDataParser.ReadJson<ProductData>("TestData/products.json");
    }

    [Test, TestCaseSource(nameof(ProductTestData))]
    public void Verify_Product_Data_Can_Be_Loaded(ProductData product)
    {
        TestContext.WriteLine($"Testing product: {product.Name}, Price: {product.Price}");
        Assert.That(product.Name, Is.Not.Empty);
        Assert.That(product.Price, Is.GreaterThan(0));
    }
}