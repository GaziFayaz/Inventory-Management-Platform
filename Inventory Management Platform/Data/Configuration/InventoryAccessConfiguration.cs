using Inventory_Management_Platform.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory_Management_Platform.Data.Configuration;

public class InventoryAccessConfiguration : IEntityTypeConfiguration<InventoryAccess>
{
  public void Configure(EntityTypeBuilder<InventoryAccess> builder)
  {
    builder.HasKey(ia => new { ia.InventoryId, ia.UserId });

    builder.HasOne(ia => ia.Inventory)
        .WithMany(i => i.AccessList)
        .HasForeignKey(ia => ia.InventoryId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(ia => ia.User)
        .WithMany(u => u.InventoryAccesses)
        .HasForeignKey(ia => ia.UserId)
        .OnDelete(DeleteBehavior.Cascade);
  }
}
