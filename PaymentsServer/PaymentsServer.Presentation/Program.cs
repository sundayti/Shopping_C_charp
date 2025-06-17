using System.Reflection;
using MediatR;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using PaymentsServer.Application;
using PaymentsServer.Domain.Interfaces;
using PaymentsServer.Infrastructure.Persistence;
using PaymentsServer.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

var connectionString = builder.Configuration.GetConnectionString("Postgres");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'Postgres' is not configured.");
}

builder.Services.AddDbContext<PaymentAccountsDbContext>(options =>
    options.UseNpgsql(connectionString)
);

builder.Services.AddScoped<IPaymentAccountRepository, PaymentAccountRepository>();
builder.Services.AddScoped<IInboxRepository, InboxRepository>();
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var applicationAssembly = typeof(ApplicationAssemblyReference).GetTypeInfo().Assembly;
builder.Services.AddMediatR(applicationAssembly);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5002, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
});

var app = builder.Build();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<PaymentAccountsDbContext>();
dbContext.Database.Migrate();

app.MapControllers();

app.Run();