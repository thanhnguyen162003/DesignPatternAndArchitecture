using CleanPatternWithCloudNative.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanPatternWithCloudNative.Infrastructure.Configurations
{
    public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder
                .ToTable("products");

            builder
                .Property(x => x.Id)
                .IsRequired(true)
                .ValueGeneratedOnAdd();

            builder
                .Property(x => x.Name)
                .IsRequired(true)
                .HasMaxLength(20);

            builder
                .Property(x => x.Description)
                .IsRequired(true)
                .HasMaxLength(200);

            builder
                .Property(x => x.CreatedAtUtc)
                .IsRequired(true);

            builder.Property(x => x.UpdatedAtUtc)
                .IsRequired(false);
        }
    }
}