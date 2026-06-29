using System.Text.Json.Serialization;

namespace ECommerce.Playwright.Framework.Api.Models;

public sealed record AuthRequest(
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("password")] string Password);

public sealed record AuthResponse(
    [property: JsonPropertyName("token")] string Token);
