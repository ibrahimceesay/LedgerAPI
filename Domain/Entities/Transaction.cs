using LedgerAPI.Domain.Enum;

namespace LedgerAPI.Domain.Entities
{
    public class Transaction
    {
        private readonly List<Entry> _entries = new();

        public Guid Id { get; private set; }
        public string Description { get; private set; } = default!;
        public string Currency { get; private set; } = default!;
        public DateTimeOffset PostedAtUtc { get; private set; }
        public string IdempotencyKey { get; private set; } = default!;


        public Guid? ReversalOfTransactionId { get; private set;  }

        public IReadOnlyCollection<Entry> Entries => _entries.AsReadOnly();

        private Transaction() { }

        private Transaction (string description, string currency, string idempotencyKey, Guid? reversalOfTransactionId)
        {
            Id = Guid.NewGuid();
            Description = description;
            Currency = currency;
            IdempotencyKey = idempotencyKey;
            ReversalOfTransactionId = reversalOfTransactionId;
            PostedAtUtc = DateTimeOffset.UtcNow;
        }

        public static Transaction Post(
            string description, 
            string currency, 
            string idempotencyKey, 
            IReadOnlyCollection<(Guid AccountId, EntrySide Side, decimal Amount)> legs, 
            Guid? reversalOfTransactionId = null)
        {
            if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new ArgumentException("Idempotency key is required.", nameof(idempotencyKey));

            //if (legs.Count < 2) throw new InsufficientEntriesException();
        }
   }
}
