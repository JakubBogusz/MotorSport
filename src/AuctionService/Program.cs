using AuctionService;
using AuctionService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDbContext>(options => 
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();
builder.Services.AddMassTransit(x => 
{
    x.AddEntityFrameworkOutbox<AuctionDbContext>(opt => 
    {
        opt.QueryDelay = TimeSpan.FromSeconds(10);

        opt.UsePostgres();
        opt.UseBusOutbox();
    });

    x.UsingRabbitMq((context, cfg) => 
    {
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();

try
{
    DbInitializer.InitDb(app);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

app.Run();
