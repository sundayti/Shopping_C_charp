using System.Reflection;
using Confluent.Kafka;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
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

// var kafkaSection = builder.Configuration.GetSection("Kafka");
// var consumerConfig = new ConsumerConfig
// {
//     BootstrapServers = kafkaSection.GetValue<string>("BootstrapServers") ?? throw new InvalidOperationException("Kafka:BootstrapServers is not configured."),
//     GroupId            = kafkaSection.GetValue<string>("GroupId") ?? "payments-group",
//     AutoOffsetReset    = AutoOffsetReset.Earliest,
//     EnableAutoCommit   = false
// };
// builder.Services.AddSingleton(consumerConfig);

// builder.Services.AddSingleton<IConsumer<string, string>>(sp =>
//     new ConsumerBuilder<string, string>(sp.GetRequiredService<ConsumerConfig>())
//         .SetKeyDeserializer(Deserializers.Utf8)
//         .SetValueDeserializer(Deserializers.Utf8)
//         .Build());

// builder.Services.AddHostedService<StupidInboxWorker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentAccountsDbContext>();
    db.Database.Migrate();
}

app.MapControllers();

app.Run();