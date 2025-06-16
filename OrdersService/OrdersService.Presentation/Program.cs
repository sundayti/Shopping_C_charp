using MediatR;
using Microsoft.EntityFrameworkCore;
using OrdersService.Infrastructure.Persistence;
using OrdersService.Infrastructure.Repositories;
using OrdersService.Infrastructure;
using OrdersService.Application.Commands;
using OrdersService.Presentation.Hubs;
using OrdersService.Presentation.GraphQL;
using OrdersService.Presentation.Middlewares;
using OrdersService.Application.DTOs;
using OrdersService.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Добавляем контроллеры и MediatR
builder.Services.AddControllers();
builder.Services.AddMediatR(typeof(CreateOrderCommandHandler).Assembly);

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrdersDatabase")));
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
builder.Services.AddScoped<ICommiter, Commiter>();

// GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<DbLoggerCategory.Query>()
    .AddMutationType<Mutation>()
    .AddType<OrderType>()
    .AddFiltering()
    .AddSorting();

// SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Эндпоинты
app.MapControllers();
app.MapGraphQL("/graphql");
app.MapHub<OrderHub>("/hubs/orders");

app.Run();