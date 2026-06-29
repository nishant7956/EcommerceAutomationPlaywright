using Microsoft.Playwright;
using OrangeHRM.Playwright.Framework.Components;
using OrangeHRM.Playwright.Framework.Config;

namespace OrangeHRM.Playwright.Framework.Pages;

public sealed class RecruitmentPage : BasePage
{
    private const string CandidatesHeader = "h5:has-text('Candidates')";

    public RecruitmentPage(IPage page, TestSettings settings) : base(page, settings)
    {
    }

    public async Task<RecruitmentPage> OpenAsync()
    {
        await new SidebarMenu(Page).OpenModuleAsync("Recruitment");
        await Waiter.WaitForVisibleAsync(CandidatesHeader);
        return this;
    }

    public async Task<bool> HasCandidateSearchFiltersAsync()
    {
        return await new SearchFilterPanel(Page).HasSearchButtonAsync();
    }
}
