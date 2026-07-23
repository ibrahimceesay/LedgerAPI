using LedgerAPI.Domain.Enum;

namespace LedgerAPI.Domain.Entities
{
    public class Entry
    {
        public Guid Id { get; private set; }
        public Guid TransactionId { get; private set;  }
        public Guid AccountId { get; private set; }
        public EntrySide Side { get; private set; }
        public decimal Amount { get; private set;  }
        public string Currency { get; private set; } = default!;


        private Entry() { }

        internal Entry(Guid transactionId, Guid accountId, EntrySide side, decimal amount, string currency)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Entry amount must be positive");

            Id = Guid.NewGuid();
            TransactionId = transactionId;
            AccountId = accountId;
            Side = side;
            Amount = amount;
            Currency = currency.ToUpperInvariant();
        }
    }
}
