using System.Text.Json.Serialization;

namespace OrangeHRM.Playwright.Framework.Api.Models;

public sealed record AuthRequest(
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("password")] string Password);

public sealed record AuthResponse(
    [property: JsonPropertyName("token")] string Token);
