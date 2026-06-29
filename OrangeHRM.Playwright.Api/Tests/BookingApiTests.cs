using System.Net;
using FluentAssertions;
using NUnit.Framework;
using OrangeHRM.Playwright.Framework.Api;
using OrangeHRM.Playwright.Framework.Api.Models;
using OrangeHRM.Playwright.Framework.Config;

using OrangeHRM.Playwright.Api.Hooks;

namespace OrangeHRM.Playwright.Api.Tests;

[TestFixture]
[Category("API")]
public sealed class BookingApiTests : BaseApiTest
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
    public async Task BookingCrud_ShouldCreateReadUpdatePatchAndDeleteBooking()
    {
        var token = await _client.CreateTokenAsync();
        var booking = NewBooking("Proof", "Concept", 275, "Breakfast");

        var createResponse = await _client.CreateBookingAsync(booking);
        createResponse.BookingId.Should().BeGreaterThan(0);
        createResponse.Booking.FirstName.Should().Be("Proof");

        var savedBooking = await _client.GetBookingAsync(createResponse.BookingId);
        savedBooking.LastName.Should().Be("Concept");

        var updatedBooking = NewBooking("Framework", "Demo", 325, "Late checkout");
        var updateResponse = await _client.UpdateBookingAsync(createResponse.BookingId, updatedBooking, token);
        updateResponse.FirstName.Should().Be("Framework");
        updateResponse.TotalPrice.Should().Be(325);

        var patchResponse = await _client.PartialUpdateBookingAsync(
            createResponse.BookingId,
            new { firstname = "Portfolio", additionalneeds = "Quiet room" },
            token);
        patchResponse.FirstName.Should().Be("Portfolio");
        patchResponse.AdditionalNeeds.Should().Be("Quiet room");

        var deleteStatus = await _client.DeleteBookingAsync(createResponse.BookingId, token);
        deleteStatus.Should().Be(HttpStatusCode.Created);
    }

    [Test]
    public async Task GetMissingBooking_ShouldReturnNotFound()
    {
        var status = await _client.GetBookingStatusAsync(99999999);

        status.Should().Be(HttpStatusCode.NotFound);
    }

    private static Booking NewBooking(string firstName, string lastName, int price, string needs)
    {
        return new Booking(
            firstName,
            lastName,
            price,
            true,
            new BookingDates("2026-07-01", "2026-07-08"),
            needs);
    }
}
