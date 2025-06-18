using Domain.Core.Entities.Provision;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Configuration;

public class ProvisionerInstanceCfg  : IEntityTypeConfiguration<ProvisionerInstance>
{
    public void Configure(EntityTypeBuilder<ProvisionerInstance> builder)
    {
        builder.ToTable("ProvisionerInstances");
        builder.HasKey(instance => instance.Id);
        builder.Property(instance => instance.InstanceName).HasMaxLength(100).IsRequired();
        builder.Property(instance => instance.IsEnabled).HasDefaultValue(true);
        builder.Property(instance => instance.CreatedAt).IsRequired();
        
        builder.HasMany(instance => instance.Settings)
            .WithOne(setting => setting.Instance)
            .HasForeignKey(setting => setting.InstanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}