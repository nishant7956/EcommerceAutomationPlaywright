namespace ECommerce.Playwright.Framework.TestData;

public sealed record PersonName(string FirstName, string MiddleName, string LastName)
{
    public string FullName => $"{FirstName} {LastName}";
}
