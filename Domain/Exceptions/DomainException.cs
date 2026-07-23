namespace LedgerAPI.Domain.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException(string message) : base (message) { }
    }

    public sealed class UnbalancedTransactionException : DomainException
    {
        public UnbalancedTransactionException(decimal debits, decimal credits) : 
            base($"Transaction is not balanced: debits={debits}, credits={credits}") { }
    }

    public sealed class InsufficientEntriesException : DomainException
    {
        public InsufficientEntriesException() : base("A transaction requires at least two entries.") { }
    }

    public sealed class CurrencyMisMatchException : DomainException
    {
        public CurrencyMisMatchException( string expected, string actual): 
            base($"All entries in a transaction must share one currency. Expected '{expected}', got '{actual}'") { }
    }
}
