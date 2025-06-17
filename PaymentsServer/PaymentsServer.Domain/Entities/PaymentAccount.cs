using PaymentsServer.Domain.ValueObjects;

namespace PaymentsServer.Domain.Entities;

public class PaymentAccount
{
    public Guid UserId { get; init; }
    public Balance Balance { get; set; } = 0;
    
    private PaymentAccount() {}

    public PaymentAccount(Guid userId)
    {
        UserId = userId;
    } 
}