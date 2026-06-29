using NUnit.Framework;
using FluentAssertions;
using Microsoft.Playwright;
using ECommerce.Playwright.Tests.Hooks;
using System.Text.Json;

namespace ECommerce.Playwright.Tests.Api.Products;

public class ProductApiTests : BaseApiTest
{
    [Test]
    public async Task Can_Get_Products_From_Api()
    {
        // Act
        var response = await Request.GetAsync("api/Products");

        // Assert
        response.Status.Should().Be(200);
        
        var jsonResponse = await response.TextAsync();
        using var doc = JsonDocument.Parse(jsonResponse);
        doc.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
    }
}
