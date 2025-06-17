using System.Net.WebSockets;
using System.Reflection;
using Confluent.Kafka;
using MediatR;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OrdersService.Application;
using OrdersService.Application.Queries;
using OrdersService.Domain.Interfaces;
using OrdersService.Domain.ValueObjects;
using OrdersService.Infrastructure.Persistence;
using OrdersService.Infrastructure.Repositories;
using OrdersService.Infrastructure.Workers;

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

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "FileAnalyticsService API", 
        Version = "v1" 
    });
    
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddSingleton<IProducer<string, string>>(_ =>
{
    var config = new ProducerConfig
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"]
    };
    return new ProducerBuilder<string, string>(config).Build();
});

builder.Services.AddSingleton<IConsumer<string, string>>(_ =>
{
    var config = new ConsumerConfig
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"],
        GroupId = builder.Configuration["Kafka:GroupId"],
        AutoOffsetReset = AutoOffsetReset.Earliest
    };
    var consumer = new ConsumerBuilder<string, string>(config).Build();
    return consumer;
});

builder.Services.AddHostedService<OutboxWorker>();
builder.Services.AddHostedService<StupidInboxWorker>();
builder.Services.AddHostedService<CleverInboxWorker>();

builder.Logging.AddConsole();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.Migrate();
}
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("v1/swagger.json", "OrdersService API v1");
    c.RoutePrefix = "swagger";
});
app.MapControllers();
app.UseWebSockets();

app.Map("/api/ws/{orderId:guid}", async (HttpContext context, Guid orderId, IMediator mediator) =>
{
    Console.WriteLine($"Incoming request: {context.Request.Path}");
    
    if (!context.WebSockets.IsWebSocketRequest)
    {
        Console.WriteLine("Not a WebSocket request!");
        Console.WriteLine("Headers:");
        foreach (var header in context.Request.Headers)
        {
            Console.WriteLine($"{header.Key}: {header.Value}");
        }
        context.Response.StatusCode = 400;
        return;
    }

    using var webSocket = await context.WebSockets.AcceptWebSocketAsync();

    var lastStatus = OrderStatus.New;
    while (webSocket.State == WebSocketState.Open)
    {
        var status = (await mediator.Send(new GetOrderStatusQuery(orderId))).Status;

        if (status != lastStatus)
        {
            var message = System.Text.Json.JsonSerializer.Serialize(new { status });
            var buffer = System.Text.Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(
                new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);

            lastStatus = status;
        }

        if (status is OrderStatus.Finished or OrderStatus.Cancelled)
        {
            break;
        }

        await Task.Delay(1000);
    }

    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Order complete", CancellationToken.None);
});

app.Run();