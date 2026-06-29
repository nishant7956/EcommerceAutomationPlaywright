using Microsoft.Playwright;

namespace ECommerce.Playwright.Framework.Components;

public sealed class SidebarMenu
{
    private readonly IPage _page;

    public SidebarMenu(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Navigates to a top-level module (e.g. 'Admin', 'PIM', 'Leave')
    /// </summary>
    public async Task OpenModuleAsync(string moduleName)
    {
        // Playwright handles text quoting automatically with the 'text=' engine,
        // but since we want the span inside the aside block, this is robust:
        var locator = _page.Locator($"aside >> text={moduleName}");
        await locator.ClickAsync();
    }
}
