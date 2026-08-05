using Unify.Erp.Domain.Accounting;

namespace Unify.Erp.Domain.Tests;

public sealed class AccountingTests
{
    [Fact]
    public void Account_normalizes_code()
    {
        var account = new Account(Guid.NewGuid(), Guid.NewGuid(), " cash ", "Cash", AccountType.Asset);

        Assert.Equal("CASH", account.Code);
        Assert.True(account.IsActive);
    }

    [Fact]
    public void Journal_line_requires_either_debit_or_credit()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new JournalLine(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Bad line", 0, 0));

        Assert.Equal("debit", exception.ParamName);
    }
}
