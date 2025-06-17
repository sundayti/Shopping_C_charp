using System.Reflection;
using Confluent.Kafka;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PaymentsServer.Application;
using PaymentsServer.Domain.Interfaces;
using PaymentsServer.Infrastructure.Persistence;
using PaymentsServer.Infrastructure.Repositories;
using PaymentsServer.Infrastructure.Workers;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

var connectionString = builder.Configuration.GetConnectionString("Postgres");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

builder.Services.AddDbContext<PaymentAccountsDbContext>(options =>
    options.UseNpgsql(connectionString, x => x.MigrationsAssembly("PaymentsServer.Infrastructure"))
);

builder.Services.AddScoped<IPaymentAccountRepository, PaymentAccountRepository>();
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
    options.ListenAnyIP(5002, listenOptions =>
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
    var cfg = new ConsumerConfig
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"],    
        GroupId = builder.Configuration["Kafka:GroupId"],             
        AutoOffsetReset  = AutoOffsetReset.Earliest,
        EnableAutoCommit = false,                               
    };

    return new ConsumerBuilder<string, string>(cfg).Build();
});

builder.Services.AddHostedService<OutboxWorkerService>();
builder.Services.AddHostedService<StupidInboxWorker>();
builder.Services.AddHostedService<CleverInboxWorker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentAccountsDbContext>();
    db.Database.Migrate();
}
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("v1/swagger.json", "OrdersService API v1");
    c.RoutePrefix = "swagger";
});
app.MapControllers();

app.Run();