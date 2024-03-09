using AuctionService.Entities;

namespace AuctionService.UnitTests;

public class AuctionEntityTests
{
    [Fact]
    public void HasReservePrice_ReservePriceGreaterThanZero_True()
    {
        var auction = new Auction{Id = Guid.NewGuid(), ReservePrice = 100};

        var result = auction.HasReservePrice();

        Assert.True(result);
    }
}