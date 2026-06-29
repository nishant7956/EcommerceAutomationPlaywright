using System.Text.Json;
using NUnit.Framework;

namespace ECommerce.Playwright.Framework.Utils;

public static class TestDataParser
{
    public static IEnumerable<TestCaseData> ReadJson<T>(string filePath)
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, filePath);
        if (!File.Exists(fullPath)) yield break;

        var json = File.ReadAllText(fullPath);
        var items = JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (items != null)
        {
            foreach (var item in items)
            {
                yield return new TestCaseData(item);
            }
        }
    }
}