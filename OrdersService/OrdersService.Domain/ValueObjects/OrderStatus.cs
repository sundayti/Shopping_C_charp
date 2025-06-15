namespace OrdersService.Domain.ValueObjects;

public enum OrderStatus : short
{
    Pending,
    Paid,
    Shipped,
    Delivered,
    Failed
}