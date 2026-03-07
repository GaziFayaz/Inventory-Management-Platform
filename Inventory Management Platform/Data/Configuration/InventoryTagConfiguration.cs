using Inventory_Management_Platform.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory_Management_Platform.Data.Configuration;

public class InventoryTagConfiguration : IEntityTypeConfiguration<InventoryTag>
{
  public void Configure(EntityTypeBuilder<InventoryTag> builder)
  {
    builder.HasKey(it => new { it.InventoryId, it.TagId });

    builder.HasOne(it => it.Inventory)
        .WithMany()
        .HasForeignKey(it => it.InventoryId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(it => it.Tag)
        .WithMany()
        .HasForeignKey(it => it.TagId)
        .OnDelete(DeleteBehavior.Cascade);
  }
}
