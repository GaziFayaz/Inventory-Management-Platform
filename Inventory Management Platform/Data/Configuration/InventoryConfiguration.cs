using Inventory_Management_Platform.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory_Management_Platform.Data.Configuration;

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
  public void Configure(EntityTypeBuilder<Inventory> builder)
  {
    builder.HasOne(i => i.Owner)
        .WithMany(u => u.OwnedInventories)
        .HasForeignKey(i => i.OwnerId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(i => i.Category)
        .WithMany(c => c.Inventories)
        .HasForeignKey(i => i.CategoryId)
        .OnDelete(DeleteBehavior.SetNull);

    // Skip navigation: inventory.Tags and tag.Inventories
    // Wired to the already-configured InventoryTag join entity.
    builder.HasMany(i => i.Tags)
        .WithMany(t => t.Inventories)
        .UsingEntity<InventoryTag>();

    // CustomIdFormat stored as JSONB — ordered list of CustomIdElement descriptors.
    builder.Property(i => i.CustomIdFormat)
        .HasColumnType("jsonb");

    // Optimistic concurrency: xmin is a Postgres system column that changes on every UPDATE.
    // Mapped as a shadow property — no Version field needed on the entity.
    builder.Property<uint>("xmin")
        .HasColumnName("xmin")
        .HasColumnType("xid")
        .ValueGeneratedOnAddOrUpdate()
        .IsConcurrencyToken();

    // Full-text search: computed tsvector column over Title + DescriptionMd with GIN index.
    builder.HasGeneratedTsVectorColumn(
            i => i.SearchVector,
            "english",
            i => new { i.Title, i.DescriptionMd })
        .HasIndex(i => i.SearchVector)
        .HasMethod("GIN");
  }
}
