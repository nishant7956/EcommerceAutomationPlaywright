namespace ECommerce.Playwright.Framework.TestData;

public static class TestDataGenerator
{
    public static PersonName UniquePerson()
    {
        var suffix = DateTime.UtcNow.ToString("HHmmssfff");
        return new PersonName($"Auto{suffix}", "PoC", $"User{Random.Shared.Next(1000, 9999)}");
    }
}
