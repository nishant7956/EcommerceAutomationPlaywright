using FluentAssertions;
using NUnit.Framework;
using ECommerce.Playwright.Framework.Api;
using ECommerce.Playwright.Framework.Api.Models;
using ECommerce.Playwright.Framework.Config;

using ECommerce.Playwright.Api.Hooks;

namespace ECommerce.Playwright.Api.Tests;

[TestFixture]
[Category("API")]
public sealed class BookingFilterApiTests : BaseApiTest
{
    private RestfulBookerClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _client = new RestfulBookerClient(TestSettings.Current.RestfulBookerBaseUrl);
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
    }

    [Test]
    public async Task GetAllBookings_ShouldReturnNonEmptyList()
    {
        var ids = await _client.GetAllBookingIdsAsync();

        ids.Should().NotBeEmpty(because: "the Restful Booker demo always has pre-seeded bookings");
        ids.Should().OnlyContain(id => id > 0, because: "all booking IDs should be positive integers");
    }

    [Test]
    public async Task GetBookingsByFirstName_ShouldReturnFilteredResults()
    {
        // Seed a booking with a unique first name so we can filter for it precisely.
        var uniqueFirstName = "FilterTest" + Guid.NewGuid().ToString("N")[..8];

        var token = await _client.CreateTokenAsync();
        var created = await _client.CreateBookingAsync(new Booking(
            uniqueFirstName, "FilterLast", 100, false,
            new BookingDates("2026-08-01", "2026-08-05"), "None"));

        try
        {
            var filtered = await _client.GetBookingsByNameAsync(firstName: uniqueFirstName);

            filtered.Should().Contain(created.BookingId,
                because: "the newly created booking should appear when filtering by its first name");
        }
        finally
        {
            await _client.DeleteBookingAsync(created.BookingId, token);
        }
    }

    [Test]
    public async Task GetBookingsByNonsenseName_ShouldReturnNoResults()
    {
        var nonsenseName = $"zzz_no_such_guest_{Guid.NewGuid():N}";

        var results = await _client.GetBookingsByNameAsync(firstName: nonsenseName);

        results.Should().BeEmpty(
            because: "a filter for a name that has never been booked should return no results");
    }

    [Test]
    public async Task GetBookingsByFullName_ShouldBeMoreSpecificThanFirstNameAlone()
    {
        var byFirstName = await _client.GetBookingsByNameAsync(firstName: "Test");
        var byFullName = await _client.GetBookingsByNameAsync(firstName: "Test", lastName: "Test");

        byFullName.Count.Should().BeLessThanOrEqualTo(byFirstName.Count,
            because: "adding a last-name filter should return the same or fewer results");
    }
}
