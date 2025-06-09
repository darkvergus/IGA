using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Configuration;

public sealed class AccountCfg : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");
        builder.HasKey(account => account.Id);
        builder.Property(identity => identity.Id).HasColumnName("id");
        builder.Property(identity => identity.CreatedAt).HasColumnName("createdAt");
        builder.Property(identity => identity.ModifiedAt).HasColumnName("modifiedAt");
        builder.Property(identity => identity.Version).IsRowVersion().HasColumnName("version");
    }
}