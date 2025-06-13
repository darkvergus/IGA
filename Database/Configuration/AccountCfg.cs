using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Configuration;

public sealed class AccountCfg : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(account => account.Id);
        builder.Property(account => account.Id).HasColumnName("Id");
        builder.Property(account => account.CreatedAt).HasColumnName("CreatedAt");
        builder.Property(account => account.ModifiedAt).HasColumnName("ModifiedAt");
        builder.Property(account => account.Version).HasColumnName("Version").IsConcurrencyToken();
        builder.Property(account => account.AttrHash).HasColumnName("AttrHash");
    }
}