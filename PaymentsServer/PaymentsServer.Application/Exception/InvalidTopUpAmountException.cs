namespace PaymentsServer.Application.Exception;

public class InvalidTopUpAmountException : System.Exception
{
    public decimal Amount { get; }

    public InvalidTopUpAmountException(decimal amount) 
        : base($"Top-up amount must be positive. Provided value was: {amount}")
    {
        Amount = amount;
    }
    
    public InvalidTopUpAmountException(string message) : base(message) { }

    public InvalidTopUpAmountException(string message, System.Exception innerException) : base(message, innerException) { }
}