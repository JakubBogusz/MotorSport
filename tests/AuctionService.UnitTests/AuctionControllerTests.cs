
using AuctionService;
using AuctionService.Controllers;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.RequestHelpers;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class AuctionControllerTests
{
  private readonly Mock<IAuctionRepository> _auctionRepo;
  private readonly Mock<IPublishEndpoint> _publishEndpoint;
  private readonly Fixture _fixture;
  private readonly AuctionsController _controller;
  private readonly IMapper _mapper;

  public AuctionControllerTests()
  {
    _fixture = new Fixture();
    _auctionRepo = new Mock<IAuctionRepository>();
    _publishEndpoint = new Mock<IPublishEndpoint>();

    var mockMapper = new MapperConfiguration(mc =>
    {
      mc.AddMaps(typeof(MappingProfiles).Assembly);
    }).CreateMapper().ConfigurationProvider;

    _mapper = new Mapper(mockMapper);
    _controller = new AuctionsController(_auctionRepo.Object, _mapper, _publishEndpoint.Object);
  }

  [Fact]
  public async Task GetAuction_WithNoParams_Returns10Auctions()
  {
    // Arrange
    var auctions = _fixture.CreateMany<AuctionDto>(10).ToList();
    _auctionRepo.Setup(repo => repo.GetAuctionsAsync(null)).ReturnsAsync(auctions);

    // Act
    var result = await _controller.GetAllAuctions(null);

    // Assert
    Assert.Equal(10, result.Value.Count);
    Assert.IsType<ActionResult<List<AuctionDto>>>(result);
  }

  [Fact]
  public async Task GetAuctionById_WithValidGuid_ReturnsAuction()
  {
    // arrange
    var auction = _fixture.Create<AuctionDto>();
    _auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

    // act
    var result = await _controller.GetAuctionById(auction.Id);

    // assert
    Assert.Equal(auction.Make, result.Value.Make);
    Assert.IsType<ActionResult<AuctionDto>>(result);
  }

  [Fact]
  public async Task GetAuctionById_WithInvalidGuid_ReturnsNotFound()
  {
    // arrange
    _auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>()))
        .ReturnsAsync(value: null);

    // act
    var result = await _controller.GetAuctionById(Guid.NewGuid());

    // assert
    Assert.IsType<NotFoundResult>(result.Result);
  }

  [Fact]
  public async Task CreateAuction_WithValidCreateAuctionDto_ReturnsCreatedAtAction()
  {
    // arrange
    var auction = _fixture.Create<CreateAuctionDto>();
    _auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
    _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

    // act
    var result = await _controller.CreateAuction(auction);
    var createdResult = result.Result as CreatedAtActionResult;

    // assert
    Assert.NotNull(createdResult);
    Assert.Equal("GetAuctionById", createdResult.ActionName);
    Assert.IsType<AuctionDto>(createdResult.Value);
  }
}