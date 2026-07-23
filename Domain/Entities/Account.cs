using LedgerAPI.Domain.Enum;

namespace LedgerAPI.Domain.Entities
{
    public class Account
    {
        public Guid Id { get; private set; }
        public string Code { get; private set; } = default!;
        public string Name { get; private set; } = default!;
        public AccountType Type { get; private set; }
        public string Currency { get; private set; } = default!;
        public bool IsActive { get; private set; } = true;
        public DateTimeOffset CreatedAtUtc { get; private set; }

        private Account() { }

        public Account(string code, string name, AccountType type, string currency)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Account code is required.", nameof(code));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Account code is required.", nameof(name));
            if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Account code is required.", nameof(currency));

            Id = Guid.NewGuid();
            Code = code;
            Name = name;
            Type = type;
            Currency = currency.ToUpperInvariant();
            CreatedAtUtc = DateTimeOffset.UtcNow;
        }


        public EntrySide NormalBalanceSide => Type switch
        {
            AccountType.Assets or AccountType.Expense => EntrySide.Debit,
            AccountType.Liability or AccountType.Equity or AccountType.Revenue => EntrySide.Credit,
            _ => throw new InvalidOperationException($"Unhandled account type '{Type}'.")
        };

        public void Deactivate() => IsActive = false;
    }
}
