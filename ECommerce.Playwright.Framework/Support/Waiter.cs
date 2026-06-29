using Microsoft.Playwright;
using ECommerce.Playwright.Framework.Config;

namespace ECommerce.Playwright.Framework.Support;

// ─────────────────────────────────────────────────────────────────────────────
//  Waiter
//  ======
//  Explicit wait helpers for complex scenarios that Playwright's built-in
//  auto-waiting does not cover.
//
//  Most standard actions (Click, Fill) auto-wait for actionability.
//  Use this class when you need to wait for a specific state, like:
//   - A specific number of elements (e.g. table rows load)
//   - An element to completely disappear (e.g. loading spinner)
//   - Specific text to be present (e.g. status changes to 'Completed')
// ─────────────────────────────────────────────────────────────────────────────

public sealed class Waiter
{
    private readonly IPage _page;
    private readonly float _defaultTimeout;

    public Waiter(IPage page, TestSettings settings)
    {
        _page = page;
        _defaultTimeout = settings.DefaultTimeoutMs;
    }

    /// <summary>
    /// Waits until the element matching the selector becomes hidden or detached from the DOM.
    /// Ideal for loading spinners, overlays, or toast messages disappearing.
    /// </summary>
    public async Task WaitForHiddenAsync(string selector, float? timeoutMs = null)
    {
        await _page.Locator(selector).WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Hidden,
            Timeout = timeoutMs ?? _defaultTimeout
        });
    }

    /// <summary>
    /// Waits until the element matching the selector becomes visible.
    /// </summary>
    public async Task WaitForVisibleAsync(string selector, float? timeoutMs = null)
    {
        await _page.Locator(selector).WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs ?? _defaultTimeout
        });
    }

    /// <summary>
    /// Waits until there are exactly <paramref name="expectedCount"/> elements matching the selector.
    /// Evaluated in the browser context via JavaScript to avoid network chatter.
    /// </summary>
    public async Task WaitForCountAsync(string selector, int expectedCount, float? timeoutMs = null)
    {
        // Playwright Assertions natively handle retry-polling. We use Expect() with a custom timeout.
        await Microsoft.Playwright.Assertions.Expect(_page.Locator(selector))
            .ToHaveCountAsync(expectedCount, new LocatorAssertionsToHaveCountOptions 
            { 
                Timeout = timeoutMs ?? _defaultTimeout 
            });
    }

    /// <summary>
    /// Waits until the element's text content matches <paramref name="expectedText"/>.
    /// </summary>
    public async Task WaitForTextAsync(string selector, string expectedText, float? timeoutMs = null)
    {
        await Microsoft.Playwright.Assertions.Expect(_page.Locator(selector))
            .ToHaveTextAsync(expectedText, new LocatorAssertionsToHaveTextOptions 
            { 
                Timeout = timeoutMs ?? _defaultTimeout 
            });
    }
    
    /// <summary>
    /// Evaluates a custom JavaScript predicate in the browser until it returns true.
    /// Example: Waiter.WaitForConditionAsync("() => document.querySelectorAll('tr').length > 5")
    /// </summary>
    public async Task WaitForConditionAsync(string javascriptPredicate, float? timeoutMs = null)
    {
        await _page.WaitForFunctionAsync(javascriptPredicate, null, new PageWaitForFunctionOptions
        {
            Timeout = timeoutMs ?? _defaultTimeout
        });
    }
}
