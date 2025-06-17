using System.Reflection;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using OrdersService.Application;
using OrdersService.Domain.Interfaces;
using OrdersService.Infrastructure.Persistence;
using OrdersService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

var connectionString = builder.Configuration.GetConnectionString("Postgres");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseNpgsql(connectionString, x => x.MigrationsAssembly("OrdersService.Infrastructure"))
);

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IInboxRepository, InboxRepository>();
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var applicationAssembly = typeof(ApplicationAssemblyReference).GetTypeInfo().Assembly;
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(applicationAssembly)
);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5001, listenOptions =>
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.Migrate();
}

app.MapControllers();

app.Run();