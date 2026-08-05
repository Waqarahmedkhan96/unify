using Unify.Erp.Domain.Finance;

namespace Unify.Erp.Domain.Tests;

public sealed class CustomerLedgerTests
{
    [Fact]
    public void Ledger_entry_balance_impact_is_debit_minus_credit()
    {
        var entry = new CustomerLedgerEntry(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            CustomerLedgerEntryType.Payment,
            "Payment",
            Guid.NewGuid(),
            0,
            100,
            DateTimeOffset.UtcNow);

        Assert.Equal(-100, entry.BalanceImpact);
    }

    [Fact]
    public void Customer_payment_normalizes_receipt_number()
    {
        var payment = new CustomerPayment(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            " rec-001 ",
            100,
            PaymentMethod.Cash,
            DateTimeOffset.UtcNow,
            null);

        Assert.Equal("REC-001", payment.ReceiptNumber);
    }
}
