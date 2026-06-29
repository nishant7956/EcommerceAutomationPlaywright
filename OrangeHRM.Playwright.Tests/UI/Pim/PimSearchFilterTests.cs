using FluentAssertions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;
using NUnit.Framework;
using OrangeHRM.Playwright.Framework.Pages;
using OrangeHRM.Playwright.Tests.Hooks;

namespace OrangeHRM.Playwright.Tests.UI.Pim;

[TestFixture]
[Category("UI")]
[Category("Regression")]
[Category("PIM")]
public sealed class PimSearchFilterTests : BaseUiTest
{
    [Test]
    public async Task SearchByNonsenseName_ShouldShowNoRecordsFound()
    {
        await LoginAsAdminAsync();

        var employeeList = await new EmployeeListPage(Page, Settings).OpenAsync();
        await employeeList.SearchByEmployeeName_NoAutocompleteAsync("zzz_nonexistent_xyz");

        // The NoResults state usually shows up quickly, but let's allow it a moment
        var noResults = await employeeList.NoResultsVisibleAsync();
        noResults.Should().BeTrue(
            because: "a search for a non-existent employee should display 'No Records Found'");
    }

    [Test]
    public async Task SearchByEmployeeId_ShouldShowAtMostOneResult()
    {
        await LoginAsAdminAsync();

        // Employee ID '0001' is the default Admin account on the OrangeHRM demo
        var employeeList = await new EmployeeListPage(Page, Settings).OpenAsync();
        
        // Let's use Playwright assertions directly to make sure the table has populated
        // with the unfiltered results first (so we don't accidentally count an empty table as 0 results prematurely)
        await Expect(employeeList.DataTable.Rows).Not.ToHaveCountAsync(0, new LocatorAssertionsToHaveCountOptions { Timeout = 10000 });
        
        await employeeList.SearchByEmployeeIdAsync("0001");

        // Now check the count
        // We know we are waiting for the loader to vanish inside SearchByEmployeeIdAsync,
        // but adding an expectation here makes it bulletproof.
        await Expect(employeeList.DataTable.Rows).ToHaveCountAsync(1, new LocatorAssertionsToHaveCountOptions { Timeout = 10000 });
    }

    [Test]
    public async Task ResetFilter_ShouldRestoreAllRecords()
    {
        await LoginAsAdminAsync();

        var employeeList = await new EmployeeListPage(Page, Settings).OpenAsync();
        await Expect(employeeList.DataTable.Rows).Not.ToHaveCountAsync(0, new LocatorAssertionsToHaveCountOptions { Timeout = 10000 });
        var totalCount = await employeeList.DataTable.RowCountAsync();

        await employeeList.SearchByEmployeeIdAsync("0001");
        
        // Using Playwright's auto-retrying logic to await the specific row count
        await Expect(employeeList.DataTable.Rows).ToHaveCountAsync(1, new LocatorAssertionsToHaveCountOptions { Timeout = 10000 });
        
        var filteredCount = await employeeList.DataTable.RowCountAsync();

        await employeeList.ResetFilterAsync();

        // It should restore the table to the original total count
        await Expect(employeeList.DataTable.Rows).ToHaveCountAsync(totalCount, new LocatorAssertionsToHaveCountOptions { Timeout = 10000 });
        
        var restoredCount = await employeeList.DataTable.RowCountAsync();
        restoredCount.Should().BeGreaterThan(filteredCount,
            because: "resetting the filter should restore more records than the filtered (ID-specific) view");
    }
}
