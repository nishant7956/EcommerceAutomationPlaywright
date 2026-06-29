using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using NUnit.Framework;
using ECommerce.Playwright.Framework.Config;
using ECommerce.Playwright.Api.Hooks;

namespace ECommerce.Playwright.Api.Tests;

// ─────────────────────────────────────────────────────────────────────────────
//  ProductApiTests
//  ===============
//  Tests for the ECommerceWebApp Products REST API:
//    GET    /api/products          → list all products
//    GET    /api/products/{id}     → get single product
//    POST   /api/products          → create a product
//    PUT    /api/products/{id}     → update a product
//    DELETE /api/products/{id}     → delete a product
// ─────────────────────────────────────────────────────────────────────────────

[TestFixture]
[Category("API")]
[Category("Smoke")]
public sealed class ProductApiTests : BaseApiTest
{
    private HttpClient _client = null!;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private string BaseUrl => TestSettings.Current.ApiBaseUrl.TrimEnd('/');

    [SetUp]
    public override Task BaseSetUp()
    {
        _client = new HttpClient { BaseAddress = new Uri(BaseUrl + "/") };
        return Task.CompletedTask;
    }

    [TearDown]
    public override Task BaseTearDown()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    // ── GET /api/products ──────────────────────────────────────────────────────

    [Test]
    [Description("GET /api/products should return 200 OK with a JSON array")]
    public async Task GetAllProducts_Returns_200_With_Json_Array()
    {
        // Act
        var response = await _client.GetAsync("api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "the products endpoint should be reachable");
        response.Content.Headers.ContentType?.MediaType.Should().Contain("json", "the response should be JSON");

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrWhiteSpace();

        using var doc = JsonDocument.Parse(body);
        doc.RootElement.ValueKind.Should().Be(JsonValueKind.Array, "the response should be a JSON array");
    }

    // ── POST /api/products ─────────────────────────────────────────────────────

    [Test]
    [Description("POST /api/products should create a new product and return 201 Created with the created entity")]
    public async Task CreateProduct_Returns_201_With_Created_Product()
    {
        // Arrange
        var newProduct = new
        {
            name = $"API Test Product {Guid.NewGuid().ToString()[..8]}",
            description = "Created via API test",
            price = 49.99,
            stockQuantity = 25
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/products", newProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created, "creating a valid product should return 201 Created");

        var created = await response.Content.ReadFromJsonAsync<ProductDto>(_jsonOptions);
        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0, "the created product should have a valid auto-assigned ID");
        created.Name.Should().Be(newProduct.name, "the returned product name should match the request");
        created.Price.Should().Be((decimal)newProduct.price);
        created.StockQuantity.Should().Be(newProduct.stockQuantity);
    }

    // ── GET /api/products/{id} ─────────────────────────────────────────────────

    [Test]
    [Description("GET /api/products/{id} for a valid product should return 200 OK with the product details")]
    public async Task GetProductById_Returns_200_With_Correct_Product()
    {
        // Arrange: Create a product first so we have a known ID
        string uniqueName = $"Get By ID Test {Guid.NewGuid().ToString()[..8]}";
        var payload = new { name = uniqueName, description = "Get by ID test", price = 10.00, stockQuantity = 1 };
        var createResponse = await _client.PostAsJsonAsync("api/products", payload);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>(_jsonOptions);

        // Act
        var response = await _client.GetAsync($"api/products/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>(_jsonOptions);
        product!.Id.Should().Be(created.Id);
        product.Name.Should().Be(uniqueName);
    }

    [Test]
    [Description("GET /api/products/{id} for a non-existent ID should return 404 Not Found")]
    public async Task GetProductById_Returns_404_For_NonExistent_Id()
    {
        // Act
        var response = await _client.GetAsync("api/products/999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "a non-existent product ID should return 404");
    }

    // ── PUT /api/products/{id} ─────────────────────────────────────────────────

    [Test]
    [Description("PUT /api/products/{id} should update the product and return 204 No Content")]
    public async Task UpdateProduct_Returns_NoContent_And_Saves_Changes()
    {
        // Arrange: Create a product to update
        string originalName = $"Original {Guid.NewGuid().ToString()[..6]}";
        var created = await CreateTestProductAsync(originalName, 5.00, 10);

        string updatedName = $"Updated {Guid.NewGuid().ToString()[..6]}";
        var updatedPayload = new
        {
            id = created.Id,
            name = updatedName,
            description = "Updated via API test",
            price = 99.99,
            stockQuantity = 50
        };

        // Act
        var putResponse = await _client.PutAsJsonAsync($"api/products/{created.Id}", updatedPayload);

        // Assert 204
        putResponse.StatusCode.Should().Be(HttpStatusCode.NoContent, "a successful PUT should return 204 No Content");

        // Verify the update was persisted
        var getResponse = await _client.GetAsync($"api/products/{created.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<ProductDto>(_jsonOptions);
        fetched!.Name.Should().Be(updatedName, "the product name should reflect the update");
        fetched.Price.Should().Be(99.99m);
    }

    // ── DELETE /api/products/{id} ──────────────────────────────────────────────

    [Test]
    [Description("DELETE /api/products/{id} should remove the product and return 204 No Content")]
    public async Task DeleteProduct_Returns_NoContent_And_Product_No_Longer_Exists()
    {
        // Arrange: Create a product to delete
        var created = await CreateTestProductAsync("To Be Deleted", 1.00, 1);

        // Act
        var deleteResponse = await _client.DeleteAsync($"api/products/{created.Id}");

        // Assert 204
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent, "deleting an existing product should return 204");

        // Verify it is gone
        var getResponse = await _client.GetAsync($"api/products/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "the deleted product should no longer be found");
    }

    // ── Validation ─────────────────────────────────────────────────────────────

    [Test]
    [Description("POST /api/products with missing required fields should return 400 Bad Request")]
    public async Task CreateProduct_With_Missing_Name_Returns_BadRequest()
    {
        // Arrange: Name is required
        var invalidPayload = new { description = "No name provided", price = 5.00, stockQuantity = 1 };

        // Act
        var response = await _client.PostAsJsonAsync("api/products", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "creating a product without a required Name field should return 400 Bad Request");
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private async Task<ProductDto> CreateTestProductAsync(string name, double price, int stock)
    {
        var payload = new { name, description = "API test product", price, stockQuantity = stock };
        var response = await _client.PostAsJsonAsync("api/products", payload);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProductDto>(_jsonOptions))!;
    }
}

// ── DTO used for JSON deserialization ──────────────────────────────────────────

internal sealed class ProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public string? ImageUrl { get; init; }
}
