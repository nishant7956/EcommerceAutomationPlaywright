using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace OrangeHRM.Playwright.Framework.Components;

public sealed class DataTableComponent
{
    private readonly IPage _page;

    // We target the row wrapper div since OrangeHRM uses divs for tables
    private readonly ILocator _tableRows;

    public DataTableComponent(IPage page)
    {
        _page = page;
        _tableRows = _page.Locator(".oxd-table-body .oxd-table-row");
    }

    /// <summary>
    /// Checks if ANY row in the table contains the expected text.
    /// Playwright's `Filter` makes this very clean.
    /// </summary>
    public async Task<bool> ContainsTextAsync(string expectedText)
    {
        // Filter rows that have the text, then check if count > 0
        var count = await _tableRows.Filter(new LocatorFilterOptions { HasText = expectedText }).CountAsync();
        return count > 0;
    }

    /// <summary>
    /// Returns the text content of all visible rows.
    /// Note: `AllInnerTextsAsync()` natively handles waiting and mapping.
    /// </summary>
    public async Task<IReadOnlyCollection<string>> RowTextsAsync()
    {
        // This evaluates directly in the browser and returns all inner texts
        var texts = await _tableRows.AllInnerTextsAsync();
        return texts.Select(t => t.Trim()).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
    }

    /// <summary>
    /// Returns the number of visible rows in the table body.
    /// Because we no longer use a stabilization loop, if you expect a specific
    /// count after an action, you should use Playwright's `Expect(locator).ToHaveCountAsync(n)`.
    /// This method is just a point-in-time snapshot.
    /// </summary>
    public async Task<int> RowCountAsync()
    {
        return await _tableRows.CountAsync();
    }
    
    /// <summary>
    /// Expose the raw locator so tests can perform auto-retrying assertions
    /// like `await Expect(dataTable.Rows).ToHaveCountAsync(5)`.
    /// </summary>
    public ILocator Rows => _tableRows;

    /// <summary>
    /// Returns true when the OrangeHRM "No Records Found" empty state is visible.
    /// Useful for asserting that a search returned zero results.
    /// </summary>
    public async Task<bool> NoResultsVisibleAsync()
    {
        var emptyState = _page.Locator("span", new PageLocatorOptions { HasTextString = "No Records Found" });
        return await emptyState.IsVisibleAsync();
    }

    /// <summary>
    /// Clicks a labelled action button (e.g., Edit, Delete) within the row whose text
    /// contains <paramref name="rowText"/>.
    /// </summary>
    public async Task ClickActionOnRowAsync(string rowText, string actionLabel)
    {
        // 1. Find the specific row containing the text
        var targetRow = _tableRows.Filter(new LocatorFilterOptions { HasText = rowText }).First;
        
        // 2. Find the action button inside that row
        // Using Regex allows case-insensitive or partial matches like XPath's normalize-space
        var actionButton = targetRow.Locator("button", new LocatorLocatorOptions { HasTextRegex = new Regex(actionLabel, RegexOptions.IgnoreCase) });
        
        // 3. Click it
        await actionButton.ClickAsync();
    }
}
