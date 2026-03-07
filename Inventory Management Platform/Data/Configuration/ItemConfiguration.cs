using Inventory_Management_Platform.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory_Management_Platform.Data.Configuration;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
  public void Configure(EntityTypeBuilder<Item> builder)
  {
    builder.HasOne(i => i.Inventory)
        .WithMany(inv => inv.Items)
        .HasForeignKey(i => i.InventoryId)
        .OnDelete(DeleteBehavior.Cascade);

    // SetNull: items survive user deletion, CreatedById becomes null.
    builder.HasOne(i => i.CreatedBy)
        .WithMany(u => u.CreatedItems)
        .HasForeignKey(i => i.CreatedById)
        .OnDelete(DeleteBehavior.SetNull);

    // Composite unique index: enforces one CustomId per Inventory at DB level.
    builder.HasIndex(i => new { i.InventoryId, i.CustomId })
        .IsUnique();

    // Optimistic concurrency: xmin shadow property — same pattern as Inventory.
    builder.Property<uint>("xmin")
        .HasColumnName("xmin")
        .HasColumnType("xid")
        .ValueGeneratedOnAddOrUpdate()
        .IsConcurrencyToken();
  }
}
