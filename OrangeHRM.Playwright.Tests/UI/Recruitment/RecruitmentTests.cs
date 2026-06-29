using FluentAssertions;
using NUnit.Framework;
using OrangeHRM.Playwright.Framework.Pages;
using OrangeHRM.Playwright.Tests.Hooks;

namespace OrangeHRM.Playwright.Tests.UI.Recruitment;

[TestFixture]
[Category("UI")]
[Category("Regression")]
[Category("Recruitment")]
public sealed class RecruitmentTests : BaseUiTest
{
    [Test]
    public async Task CandidateList_ShouldExposeSearchFilters()
    {
        await LoginAsAdminAsync();

        var recruitmentPage = await new RecruitmentPage(Page, Settings).OpenAsync();

        var hasFilters = await recruitmentPage.HasCandidateSearchFiltersAsync();
        hasFilters.Should().BeTrue();
    }
}
