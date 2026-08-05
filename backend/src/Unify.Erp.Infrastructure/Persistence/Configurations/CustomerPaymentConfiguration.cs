using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unify.Erp.Domain.Branches;
using Unify.Erp.Domain.Customers;
using Unify.Erp.Domain.Finance;
using Unify.Erp.Domain.Organisations;
using Unify.Erp.Domain.Sales;

namespace Unify.Erp.Infrastructure.Persistence.Configurations;

public sealed class CustomerPaymentConfiguration :
    IEntityTypeConfiguration<CustomerLedgerEntry>,
    IEntityTypeConfiguration<CustomerPayment>,
    IEntityTypeConfiguration<PaymentAllocation>
{
    public void Configure(EntityTypeBuilder<CustomerLedgerEntry> builder)
    {
        builder.ToTable("customer_ledger_entries");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.OrganisationId).HasColumnName("organisation_id").IsRequired();
        builder.Property(entity => entity.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(entity => entity.EntryType).HasColumnName("entry_type").IsRequired();
        builder.Property(entity => entity.ReferenceType).HasColumnName("reference_type").HasMaxLength(60).IsRequired();
        builder.Property(entity => entity.ReferenceId).HasColumnName("reference_id").IsRequired();
        builder.Property(entity => entity.Debit).HasColumnName("debit").HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.Credit).HasColumnName("credit").HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.EntryDateUtc).HasColumnName("entry_date_utc").IsRequired();
        builder.Property(entity => entity.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Ignore(entity => entity.BalanceImpact);
        builder.HasIndex(entity => new { entity.OrganisationId, entity.CustomerId, entity.EntryDateUtc });
        RestrictToOrganisation(builder, "fk_customer_ledger_entries_organisations_organisation_id");
        builder.HasOne<Customer>().WithMany().HasForeignKey(entity => entity.CustomerId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_customer_ledger_entries_customers_customer_id");
    }

    public void Configure(EntityTypeBuilder<CustomerPayment> builder)
    {
        builder.ToTable("customer_payments");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.OrganisationId).HasColumnName("organisation_id").IsRequired();
        builder.Property(entity => entity.BranchId).HasColumnName("branch_id").IsRequired();
        builder.Property(entity => entity.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(entity => entity.ReceiptNumber).HasColumnName("receipt_number").HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.Method).HasColumnName("method").IsRequired();
        builder.Property(entity => entity.PaymentDateUtc).HasColumnName("payment_date_utc").IsRequired();
        builder.Property(entity => entity.Notes).HasColumnName("notes").HasMaxLength(500);
        builder.Property(entity => entity.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.HasIndex(entity => new { entity.OrganisationId, entity.ReceiptNumber }).IsUnique();
        builder.HasIndex(entity => new { entity.OrganisationId, entity.CustomerId });
        RestrictToOrganisation(builder, "fk_customer_payments_organisations_organisation_id");
        builder.HasOne<Customer>().WithMany().HasForeignKey(entity => entity.CustomerId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_customer_payments_customers_customer_id");
        builder.HasOne<Branch>().WithMany().HasForeignKey(entity => entity.BranchId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_customer_payments_branches_branch_id");
    }

    public void Configure(EntityTypeBuilder<PaymentAllocation> builder)
    {
        builder.ToTable("payment_allocations");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.OrganisationId).HasColumnName("organisation_id").IsRequired();
        builder.Property(entity => entity.PaymentId).HasColumnName("payment_id").IsRequired();
        builder.Property(entity => entity.SaleId).HasColumnName("sale_id").IsRequired();
        builder.Property(entity => entity.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
        builder.HasIndex(entity => new { entity.OrganisationId, entity.PaymentId });
        builder.HasIndex(entity => new { entity.OrganisationId, entity.SaleId });
        RestrictToOrganisation(builder, "fk_payment_allocations_organisations_organisation_id");
        builder.HasOne<CustomerPayment>().WithMany().HasForeignKey(entity => entity.PaymentId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_payment_allocations_customer_payments_payment_id");
        builder.HasOne<Sale>().WithMany().HasForeignKey(entity => entity.SaleId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_payment_allocations_sales_sale_id");
    }

    private static void RestrictToOrganisation<TEntity>(EntityTypeBuilder<TEntity> builder, string constraintName)
        where TEntity : class
    {
        builder.HasOne<Organisation>()
            .WithMany()
            .HasForeignKey("OrganisationId")
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName(constraintName);
    }
}
