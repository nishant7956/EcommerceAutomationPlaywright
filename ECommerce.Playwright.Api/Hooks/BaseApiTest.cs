using NUnit.Framework;

namespace ECommerce.Playwright.Api.Hooks;

[TestFixture]
public abstract class BaseApiTest
{
    [SetUp]
    public virtual Task BaseSetUp()
    {
        // Common API setup can go here
        return Task.CompletedTask;
    }

    [TearDown]
    public virtual Task BaseTearDown()
    {
        // Common API teardown can go here
        return Task.CompletedTask;
    }
}
