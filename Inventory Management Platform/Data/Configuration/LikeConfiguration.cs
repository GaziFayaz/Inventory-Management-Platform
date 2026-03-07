using Inventory_Management_Platform.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory_Management_Platform.Data.Configuration;

public class LikeConfiguration : IEntityTypeConfiguration<Like>
{
  public void Configure(EntityTypeBuilder<Like> builder)
  {
    builder.HasKey(l => new { l.ItemId, l.UserId });

    builder.HasOne(l => l.Item)
        .WithMany(i => i.Likes)
        .HasForeignKey(l => l.ItemId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(l => l.User)
        .WithMany(u => u.Likes)
        .HasForeignKey(l => l.UserId)
        .OnDelete(DeleteBehavior.Cascade);
  }
}
