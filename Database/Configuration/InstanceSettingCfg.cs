using Domain.Core.Entities.Provision;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Configuration;

public sealed class InstanceSettingCfg : IEntityTypeConfiguration<InstanceSetting>
{
    public void Configure(EntityTypeBuilder<InstanceSetting> builder)
    {
        builder.ToTable("ProvisionSettings");
        builder.HasKey(setting => setting.Id);
        builder.Property(setting => setting.Key).HasMaxLength(100).IsRequired();
        builder.Property(setting => setting.Value).HasColumnType("nvarchar(max)").IsRequired();
    }
}