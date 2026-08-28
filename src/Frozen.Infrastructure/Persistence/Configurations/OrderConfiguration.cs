using Frozen.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Frozen.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(o => o.OrderNumber).IsUnique();

        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(50);

        builder.Property(o => o.ShippingFullName).IsRequired().HasMaxLength(200);
        builder.Property(o => o.ShippingAddressLine1).IsRequired().HasMaxLength(300);
        builder.Property(o => o.ShippingAddressLine2).HasMaxLength(300);
        builder.Property(o => o.ShippingCity).IsRequired().HasMaxLength(150);
        builder.Property(o => o.ShippingState).HasMaxLength(150);
        builder.Property(o => o.ShippingPostalCode).IsRequired().HasMaxLength(20);
        builder.Property(o => o.ShippingCountry).HasMaxLength(100);
        builder.Property(o => o.ShippingPhone).HasMaxLength(30);
        builder.Property(o => o.Notes).HasMaxLength(1000);

        builder.Property(o => o.SubTotal).HasColumnType("decimal(18,2)");
        builder.Property(o => o.ShippingCost).HasColumnType("decimal(18,2)");
        builder.Property(o => o.Tax).HasColumnType("decimal(18,2)");
        builder.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.Property(i => i.ProductName).IsRequired().HasMaxLength(300);
        builder.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");

        builder.Ignore(i => i.LineTotal);

        builder.HasOne(i => i.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
