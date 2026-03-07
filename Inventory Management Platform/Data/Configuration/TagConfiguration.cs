using Inventory_Management_Platform.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory_Management_Platform.Data.Configuration;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
  public void Configure(EntityTypeBuilder<Tag> builder)
  {
    // Unique + btree index: enforces tag name uniqueness and enables fast prefix autocomplete.
    builder.HasIndex(t => t.Name)
        .IsUnique();
  }
}
