using LedgerAPI.Domain.Enum;
using LedgerAPI.Domain.Exceptions;

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

            if (legs.Count < 2) throw new InsufficientEntriesException();

            var normalizedCurrency = currency.ToUpperInvariant();

            var transaction = new Transaction(description, normalizedCurrency, idempotencyKey, reversalOfTransactionId);

            decimal debitTotal = 0m;
            decimal creditTotal = 0m;

            foreach (var leg in legs)
            {
                var entry = new Entry(transaction.Id, leg.AccountId, leg.Side, leg.Amount, normalizedCurrency);
                transaction._entries.Add(entry);

                if (leg.Side == EntrySide.Debit) debitTotal += leg.Amount;
                else creditTotal += leg.Amount;
            }

            if (debitTotal != creditTotal) throw new UnbalancedTransactionException(debitTotal, creditTotal);

            return transaction;
        }

        public Transaction Reverse(string idempotencyKey, string? reasonSuffix = null)
        {
            var flippedLegs = _entries
                .Select(e => (e.AccountId, Side: e.Side == EntrySide.Debit ? EntrySide.Credit : EntrySide.Debit, e.Amount))
                .ToList();

            var description = reasonSuffix is null
                ? $"Reversal of {Id}: {Description}"
                : $"Reversal of {Id}: {Description} ({reasonSuffix})";

            return Post(description, Currency, idempotencyKey, flippedLegs, reversalOfTransactionId: Id);
        }
    }
}
