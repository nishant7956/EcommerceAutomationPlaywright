using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using OrangeHRM.Playwright.Framework.Api.Models;

namespace OrangeHRM.Playwright.Framework.Api;

public sealed class RestfulBookerClient : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _client;

    public RestfulBookerClient(string baseUrl)
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/")
        };

        _client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    }

    public async Task<string> CreateTokenAsync(string username = "admin", string password = "password123")
    {
        var response = await _client.PostAsJsonAsync("auth", new AuthRequest(username, password), JsonOptions);
        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        return auth?.Token ?? throw new InvalidOperationException("Restful Booker did not return an auth token.");
    }

    public async Task<BookingCreateResponse> CreateBookingAsync(Booking booking)
    {
        var response = await _client.PostAsJsonAsync("booking", booking, JsonOptions);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<BookingCreateResponse>(JsonOptions)
            ?? throw new InvalidOperationException("Restful Booker did not return a booking create response.");
    }

    public async Task<Booking> GetBookingAsync(int bookingId)
    {
        var response = await _client.GetAsync($"booking/{bookingId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Booking>(JsonOptions)
            ?? throw new InvalidOperationException($"Booking {bookingId} response was empty.");
    }

    public async Task<Booking> UpdateBookingAsync(int bookingId, Booking booking, string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"booking/{bookingId}")
        {
            Content = JsonContent.Create(booking, options: JsonOptions)
        };
        request.Headers.Add("Cookie", $"token={token}");

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Booking>(JsonOptions)
            ?? throw new InvalidOperationException($"Booking {bookingId} update response was empty.");
    }

    public async Task<Booking> PartialUpdateBookingAsync(int bookingId, object patch, string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, $"booking/{bookingId}")
        {
            Content = new StringContent(JsonSerializer.Serialize(patch, JsonOptions), Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Cookie", $"token={token}");

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Booking>(JsonOptions)
            ?? throw new InvalidOperationException($"Booking {bookingId} partial update response was empty.");
    }

    public async Task<HttpStatusCode> DeleteBookingAsync(int bookingId, string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"booking/{bookingId}");
        request.Headers.Add("Cookie", $"token={token}");

        var response = await _client.SendAsync(request);
        return response.StatusCode;
    }

    public async Task<HttpStatusCode> GetBookingStatusAsync(int bookingId)
    {
        var response = await _client.GetAsync($"booking/{bookingId}");
        return response.StatusCode;
    }

    /// <summary>Returns all booking IDs from GET /booking.</summary>
    public async Task<IReadOnlyList<int>> GetAllBookingIdsAsync()
    {
        var response = await _client.GetAsync("booking");
        response.EnsureSuccessStatusCode();

        var results = await response.Content.ReadFromJsonAsync<BookingIdItem[]>(JsonOptions);
        return results?.Select(r => r.BookingId).ToList() ?? [];
    }

    /// <summary>Returns booking IDs filtered by first and/or last name query params.</summary>
    public async Task<IReadOnlyList<int>> GetBookingsByNameAsync(string? firstName = null, string? lastName = null)
    {
        var query = new List<string>();
        if (firstName is not null) query.Add($"firstname={Uri.EscapeDataString(firstName)}");
        if (lastName is not null) query.Add($"lastname={Uri.EscapeDataString(lastName)}");

        var url = query.Count > 0 ? $"booking?{string.Join("&", query)}" : "booking";
        var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var results = await response.Content.ReadFromJsonAsync<BookingIdItem[]>(JsonOptions);
        return results?.Select(r => r.BookingId).ToList() ?? [];
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
