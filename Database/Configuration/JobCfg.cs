using Domain.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Configuration;

public class JobCfg : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("Jobs");
        builder.HasKey(job => job.Id);
        builder.Property(job => job.Id).HasColumnName("Id");
        builder.Property(job => job.Type).HasColumnName("Type").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(job => job.ConnectorName).HasColumnName("ConnectorName").IsRequired();
        builder.Property(job => job.ConnectorInstanceId).HasColumnName("ConnectorInstanceId").IsRequired();
        builder.Property(job => job.PayloadJson).HasColumnName("PayloadJson").HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(job => job.Status).HasColumnName("Status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(job => job.CreatedAt).HasColumnName("CreatedAt").IsRequired();
        builder.Property(job => job.StartedAt).HasColumnName("StartedAt");
        builder.Property(job => job.FinishedAt).HasColumnName("FinishedAt");
        builder.Property(job => job.Error).HasColumnName("Error").HasColumnType("nvarchar(max)");
        builder.HasIndex(job => new { job.Status, job.Type, job.ConnectorInstanceId });
    }
}