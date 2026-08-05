using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unify.Erp.Domain.Devices;

namespace Unify.Erp.Infrastructure.Persistence.Configurations;

public sealed class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("devices");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.OrganisationId).HasColumnName("organisation_id").IsRequired();
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
        builder.Property(entity => entity.Type).HasColumnName("type").IsRequired();
        builder.Property(entity => entity.Status).HasColumnName("status").IsRequired();

        builder.Ignore(entity => entity.CanSynchronize);

        builder.HasIndex(entity => entity.OrganisationId);
    }
}
