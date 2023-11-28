using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService
{
    public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
    {
        private readonly IMapper _mapper;

        public AuctionUpdatedConsumer(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task Consume(ConsumeContext<AuctionUpdated> context)
        {
            Console.WriteLine("--> Consuming auction updated: " + context.Message.Id);

            var item = _mapper.Map<Item>(context.Message);
            
            // TODO - write a Linq query for MongoDB which updates the specific Auction
            // var result = await DB.Update<Item>()
            
            await item.SaveAsync();
        }
    }
}