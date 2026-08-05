using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unify.Erp.Domain.Accounting;
using Unify.Erp.Domain.Organisations;

namespace Unify.Erp.Infrastructure.Persistence.Configurations;

public sealed class AccountingConfiguration :
    IEntityTypeConfiguration<Account>,
    IEntityTypeConfiguration<FiscalPeriod>,
    IEntityTypeConfiguration<JournalEntry>,
    IEntityTypeConfiguration<JournalLine>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.OrganisationId).HasColumnName("organisation_id").IsRequired();
        builder.Property(entity => entity.Code).HasColumnName("code").HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
        builder.Property(entity => entity.Type).HasColumnName("type").IsRequired();
        builder.Property(entity => entity.Status).HasColumnName("status").IsRequired();
        builder.Ignore(entity => entity.IsActive);
        builder.HasIndex(entity => new { entity.OrganisationId, entity.Code }).IsUnique();
        RestrictOrganisation(builder, "fk_accounts_organisations_organisation_id");
    }

    public void Configure(EntityTypeBuilder<FiscalPeriod> builder)
    {
        builder.ToTable("fiscal_periods");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.OrganisationId).HasColumnName("organisation_id").IsRequired();
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
        builder.Property(entity => entity.StartsOn).HasColumnName("starts_on").IsRequired();
        builder.Property(entity => entity.EndsOn).HasColumnName("ends_on").IsRequired();
        builder.Property(entity => entity.Status).HasColumnName("status").IsRequired();
        builder.Ignore(entity => entity.IsOpen);
        builder.HasIndex(entity => new { entity.OrganisationId, entity.StartsOn, entity.EndsOn });
        RestrictOrganisation(builder, "fk_fiscal_periods_organisations_organisation_id");
    }

    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("journal_entries");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.OrganisationId).HasColumnName("organisation_id").IsRequired();
        builder.Property(entity => entity.FiscalPeriodId).HasColumnName("fiscal_period_id").IsRequired();
        builder.Property(entity => entity.JournalNumber).HasColumnName("journal_number").HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.JournalDate).HasColumnName("journal_date").IsRequired();
        builder.Property(entity => entity.Description).HasColumnName("description").HasMaxLength(240).IsRequired();
        builder.Property(entity => entity.Status).HasColumnName("status").IsRequired();
        builder.Property(entity => entity.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.HasIndex(entity => new { entity.OrganisationId, entity.JournalNumber }).IsUnique();
        builder.HasOne<FiscalPeriod>().WithMany().HasForeignKey(entity => entity.FiscalPeriodId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_journal_entries_fiscal_periods_fiscal_period_id");
        RestrictOrganisation(builder, "fk_journal_entries_organisations_organisation_id");
    }

    public void Configure(EntityTypeBuilder<JournalLine> builder)
    {
        builder.ToTable("journal_lines");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.OrganisationId).HasColumnName("organisation_id").IsRequired();
        builder.Property(entity => entity.JournalEntryId).HasColumnName("journal_entry_id").IsRequired();
        builder.Property(entity => entity.AccountId).HasColumnName("account_id").IsRequired();
        builder.Property(entity => entity.Description).HasColumnName("description").HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.Debit).HasColumnName("debit").HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.Credit).HasColumnName("credit").HasPrecision(18, 2).IsRequired();
        builder.HasIndex(entity => new { entity.OrganisationId, entity.JournalEntryId });
        builder.HasOne<JournalEntry>().WithMany().HasForeignKey(entity => entity.JournalEntryId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_journal_lines_journal_entries_journal_entry_id");
        builder.HasOne<Account>().WithMany().HasForeignKey(entity => entity.AccountId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_journal_lines_accounts_account_id");
        RestrictOrganisation(builder, "fk_journal_lines_organisations_organisation_id");
    }

    private static void RestrictOrganisation<TEntity>(EntityTypeBuilder<TEntity> builder, string constraintName)
        where TEntity : class
    {
        builder.HasOne<Organisation>().WithMany().HasForeignKey("OrganisationId")
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName(constraintName);
    }
}
