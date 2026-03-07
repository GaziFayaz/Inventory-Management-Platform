using Inventory_Management_Platform.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory_Management_Platform.Data.Configuration;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
  public void Configure(EntityTypeBuilder<Post> builder)
  {
    builder.HasOne(p => p.Inventory)
        .WithMany(i => i.Posts)
        .HasForeignKey(p => p.InventoryId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(p => p.Author)
        .WithMany(u => u.Posts)
        .HasForeignKey(p => p.AuthorId)
        .OnDelete(DeleteBehavior.Cascade);
  }
}
