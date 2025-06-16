using System.Text.Json.Serialization;

namespace OrdersService.Domain.ValueObjects;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStatus : short
{
    New,
    Finished,
    Cancelled
}