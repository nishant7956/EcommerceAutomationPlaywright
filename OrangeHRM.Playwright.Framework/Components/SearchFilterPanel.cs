using Microsoft.Playwright;

namespace OrangeHRM.Playwright.Framework.Components;

public sealed class SearchFilterPanel
{
    private readonly IPage _page;

    public SearchFilterPanel(IPage page)
    {
        _page = page;
    }

    public async Task SearchAsync()
    {
        await Button("Search").ClickAsync();
    }

    public async Task ResetAsync()
    {
        await Button("Reset").ClickAsync();
    }

    public async Task<bool> HasSearchButtonAsync()
    {
        return await Button("Search").IsVisibleAsync();
    }

    private ILocator Button(string text)
    {
        return _page.Locator($"button:has-text('{text}')");
    }
}
