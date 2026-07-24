using LedgerAPI.Domain.Entities;
using LedgerAPI.Domain.Enum;
using LedgerAPI.Domain.Exceptions;
using Xunit;

namespace Ledger.Domain.Tests;

public class TransactionInvariantTests
{
    private static readonly Guid CashAccount = Guid.NewGuid();
    private static readonly Guid RevenueAccount = Guid.NewGuid();
    private static readonly Guid ExpenseAccount = Guid.NewGuid();

    [Fact]
    public void Post_WithBalancedLegs_Succeeds()
    {
        var txn = Transaction.Post(
            description: "Cash sale",
            currency: "GMD",
            idempotencyKey: "key-1",
            legs: new (Guid, EntrySide, decimal)[]
            {
                (CashAccount, EntrySide.Debit, 100m),
                (RevenueAccount, EntrySide.Credit, 100m)
            });

        Assert.Equal(2, txn.Entries.Count);
        Assert.Equal("GMD", txn.Currency);
    }

    [Fact]
    public void Post_WithUnbalancedLegs_ThrowsUnbalancedTransactionException()
    {
        Assert.Throws<UnbalancedTransactionException>(() =>
            Transaction.Post(
                description: "Bad entry",
                currency: "GMD",
                idempotencyKey: "key-2",
                legs: new (Guid, EntrySide, decimal)[]
                {
                    (CashAccount, EntrySide.Debit, 100m),
                    (RevenueAccount, EntrySide.Credit, 90m)
                }));
    }

    [Fact]
    public void Post_WithFewerThanTwoLegs_ThrowsInsufficientEntriesException()
    {
        Assert.Throws<InsufficientEntriesException>(() =>
            Transaction.Post(
                description: "Single leg",
                currency: "GMD",
                idempotencyKey: "key-3",
                legs: new (Guid, EntrySide, decimal)[]
                {
                    (CashAccount, EntrySide.Debit, 100m)
                }));
    }

    [Fact]
    public void Post_WithMultiLegSplit_SucceedsWhenTotalsMatch()
    {
        // one debit split across two credits — a valid real-world shape
        // (e.g. a sale that is partly revenue, partly sales tax payable)
        var taxAccount = Guid.NewGuid();

        var txn = Transaction.Post(
            description: "Sale with tax",
            currency: "GMD",
            idempotencyKey: "key-4",
            legs: new (Guid, EntrySide, decimal)[]
            {
                (CashAccount, EntrySide.Debit, 110m),
                (RevenueAccount, EntrySide.Credit, 100m),
                (taxAccount, EntrySide.Credit, 10m)
            });

        Assert.Equal(3, txn.Entries.Count);
    }

    [Fact]
    public void Post_WithoutIdempotencyKey_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            Transaction.Post(
                description: "No key",
                currency: "GMD",
                idempotencyKey: "",
                legs: new (Guid, EntrySide, decimal)[]
                {
                    (CashAccount, EntrySide.Debit, 100m),
                    (RevenueAccount, EntrySide.Credit, 100m)
                }));
    }

    [Fact]
    public void Reverse_FlipsEveryLegAndBalances()
    {
        var original = Transaction.Post(
            description: "Expense payment",
            currency: "GMD",
            idempotencyKey: "key-5",
            legs: new (Guid, EntrySide, decimal)[]
            {
                (ExpenseAccount, EntrySide.Debit, 50m),
                (CashAccount, EntrySide.Credit, 50m)
            });

        var reversal = original.Reverse("key-5-reversal", reasonSuffix: "duplicate charge");

        Assert.Equal(original.Id, reversal.ReversalOfTransactionId);
        Assert.Equal(2, reversal.Entries.Count);
        Assert.All(reversal.Entries, e =>
        {
            var originalEntry = original.Entries.Single(oe => oe.AccountId == e.AccountId);
            Assert.NotEqual(originalEntry.Side, e.Side);
            Assert.Equal(originalEntry.Amount, e.Amount);
        });
    }

    [Fact]
    public void Entries_Collection_CannotBeMutatedFromOutside()
    {
        var txn = Transaction.Post(
            description: "Immutability check",
            currency: "GMD",
            idempotencyKey: "key-6",
            legs: new (Guid, EntrySide, decimal)[]
            {
                (CashAccount, EntrySide.Debit, 20m),
                (RevenueAccount, EntrySide.Credit, 20m)
            });

        // Entries is IReadOnlyCollection<Entry> — this line intentionally
        // would not compile if uncommented, which is the point:
        // txn.Entries.Add(someEntry);
        Assert.IsAssignableFrom<IReadOnlyCollection<Entry>>(txn.Entries);
    }
}