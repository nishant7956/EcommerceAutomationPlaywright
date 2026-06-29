using Microsoft.Playwright;
using ECommerce.Playwright.Framework.Config;

namespace ECommerce.Playwright.Framework.Pages;

// ─────────────────────────────────────────────────────────────────────────────
//  BasePage
//  ========
//  The base class for all Page Object Models (POM).
//
//  Key changes from Selenium:
//  - Uses IPage instead of IWebDriver.
//  - Everything is async/await (returning Task or Task<T>).
//  - Selectors are strings (e.g. "css=.btn", "text=Login") instead of By objects.
//  - Playwright's native auto-wait eliminates most explicit Waiter calls.
// ─────────────────────────────────────────────────────────────────────────────

public abstract class BasePage
{
    protected readonly IPage Page;
    protected readonly TestSettings Settings;
    protected readonly Support.Waiter Waiter;

    protected BasePage(IPage page, TestSettings settings)
    {
        Page = page;
        Settings = settings;
        Waiter = new Support.Waiter(page, settings);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Navigation
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Navigates to a path relative to the BaseUrl defined in TestSettings.
    /// Playwright automatically waits for the load state (default: Load) to be reached.
    /// </summary>
    protected async Task NavigateToAsync(string relativePath)
    {
        var baseUri = new Uri(Settings.BaseUrl.TrimEnd('/') + "/");
        var targetUri = new Uri(baseUri, relativePath.TrimStart('/'));
        
        await Page.GotoAsync(targetUri.ToString());
    }

    /// <summary>
    /// Playwright equivalent to waiting for document.readyState == 'complete'.
    /// </summary>
    protected async Task WaitForLoadStateAsync(LoadState state = LoadState.NetworkIdle)
    {
        await Page.WaitForLoadStateAsync(state);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Action Helpers
    // ─────────────────────────────────────────────────────────────────────────
    //  Playwright's ILocator methods (ClickAsync, FillAsync) already include 
    //  built-in actionability checks (visible, stable, enabled, receives events).
    //  These helpers just reduce boilerplate.

    /// <summary>
    /// Clicks an element. Playwright automatically waits for it to be visible and stable.
    /// </summary>
    protected async Task ClickAsync(string selector)
    {
        await Page.Locator(selector).ClickAsync();
    }

    /// <summary>
    /// Fills an input field. Playwright automatically clears existing text first!
    /// This replaces Selenium's SendKeys(Keys.Control + "a") + Backspace hack.
    /// </summary>
    protected async Task TypeAsync(string selector, string value)
    {
        await Page.Locator(selector).FillAsync(value);
    }

    /// <summary>
    /// Selects an option from a native &lt;select&gt; dropdown by its visible text.
    /// </summary>
    protected async Task SelectDropdownAsync(string selector, string visibleText)
    {
        await Page.Locator(selector).SelectOptionAsync(new SelectOptionValue { Label = visibleText });
    }

    /// <summary>
    /// Scrolls the element into view if it is not already visible in the viewport.
    /// </summary>
    protected async Task ScrollIntoViewAsync(string selector)
    {
        await Page.Locator(selector).ScrollIntoViewIfNeededAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Data Extraction & State
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Retrieves the text content of an element.
    /// </summary>
    protected async Task<string> TextOfAsync(string selector)
    {
        return (await Page.Locator(selector).InnerTextAsync()).Trim();
    }

    /// <summary>
    /// Waits for the element to become visible. Returns true if it appears within the timeout,
    /// or false if it times out. This matches the legacy Selenium behavior.
    /// </summary>
    protected async Task<bool> IsVisibleAsync(string selector, float? timeoutMs = null)
    {
        return await IsVisibleAsync(Page.Locator(selector), timeoutMs);
    }

    /// <summary>
    /// Waits for the element to become visible. Returns true if it appears within the timeout,
    /// or false if it times out.
    /// </summary>
    protected async Task<bool> IsVisibleAsync(ILocator locator, float? timeoutMs = null)
    {
        try
        {
            await locator.WaitForAsync(new LocatorWaitForOptions 
            { 
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs ?? Settings.DefaultTimeoutMs 
            });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }
}
