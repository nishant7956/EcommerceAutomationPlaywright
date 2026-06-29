using Microsoft.Playwright;
using ECommerce.Playwright.Framework.Config;

namespace ECommerce.Playwright.Framework.Pages;

public class ProductsPage : BasePage
{
    private ILocator CreateProductButton => Page.Locator("#btn-create-product");
    private ILocator ProductsTable => Page.Locator("#products-table");
    
    // Create Form
    private ILocator NameInput => Page.Locator("#product-name");
    private ILocator DescriptionInput => Page.Locator("#product-description");
    private ILocator PriceInput => Page.Locator("#product-price");
    private ILocator StockInput => Page.Locator("#product-stock");
    private ILocator SubmitButton => Page.Locator("#btn-submit-product");

    public ProductsPage(IPage page, TestSettings settings) : base(page, settings)
    {
    }

    public async Task NavigateAsync()
    {
        await Page.GotoAsync($"{Settings.BaseUrl}/Products");
        await WaitForLoadStateAsync();
    }

    public async Task ClickCreateProductAsync()
    {
        await CreateProductButton.ClickAsync();
        await WaitForLoadStateAsync();
    }

    public async Task FillProductDetailsAsync(string name, string description, string price, string stock)
    {
        await NameInput.FillAsync(name);
        await DescriptionInput.FillAsync(description);
        await PriceInput.FillAsync(price);
        await StockInput.FillAsync(stock);
    }

    public async Task SubmitProductAsync()
    {
        await SubmitButton.ClickAsync();
        await WaitForLoadStateAsync();
    }
    
    public async Task<bool> IsProductInTableAsync(string name)
    {
        var row = ProductsTable.Locator("tbody tr").Filter(new LocatorFilterOptions { HasText = name });
        return await row.CountAsync() > 0;
    }
}
