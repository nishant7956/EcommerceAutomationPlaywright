using FluentAssertions;
using NUnit.Framework;
using OrangeHRM.Playwright.Framework.Pages;
using OrangeHRM.Playwright.Framework.TestData;
using OrangeHRM.Playwright.Tests.Hooks;

namespace OrangeHRM.Playwright.Tests.UI.Pim;

[TestFixture]
[Category("UI")]
[Category("Regression")]
[Category("PIM")]
public sealed class PimEmployeeTests : BaseUiTest
{
    [Test]
    public async Task AddEmployee_ShouldCreateEmployeeProfile()
    {
        var employee = TestDataGenerator.UniquePerson();
        await LoginAsAdminAsync();

        var pimPage = await new PimPage(Page, Settings).OpenAsync();
        var addEmployeePage = await pimPage.GoToAddEmployeeAsync();
        
        await addEmployeePage.FillNameAsync(employee.FirstName, employee.MiddleName, employee.LastName);
        var detailsPage = await addEmployeePage.SaveAsync();

        var isLoaded = await detailsPage.IsLoadedAsync();
        isLoaded.Should().BeTrue();
    }

    [Test]
    public async Task SearchEmployee_ShouldFindCreatedEmployee()
    {
        var employee = TestDataGenerator.UniquePerson();
        await LoginAsAdminAsync();

        // 1. Create the employee
        var pimPage = await new PimPage(Page, Settings).OpenAsync();
        var addEmployeePage = await pimPage.GoToAddEmployeeAsync();
        
        await addEmployeePage.FillNameAsync(employee.FirstName, employee.MiddleName, employee.LastName);
        await addEmployeePage.SaveAsync();

        // 2. Search for the employee
        var employeeList = await new EmployeeListPage(Page, Settings).OpenAsync();
        await employeeList.SearchByEmployeeNameAsync(employee.FirstName);

        var hasEmployee = await employeeList.HasEmployeeAsync(employee.FirstName);
        hasEmployee.Should().BeTrue();
    }
}
