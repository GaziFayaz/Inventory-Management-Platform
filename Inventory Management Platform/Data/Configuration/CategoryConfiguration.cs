using Inventory_Management_Platform.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory_Management_Platform.Data.Configuration;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
  public void Configure(EntityTypeBuilder<Category> builder)
  {
    builder.HasData(
        new Category { Id = 1, Name = "Office Equipment" },
        new Category { Id = 2, Name = "Books" },
        new Category { Id = 3, Name = "HR Documents" },
        new Category { Id = 4, Name = "Software Licenses" },
        new Category { Id = 5, Name = "Furniture" },
        new Category { Id = 6, Name = "Electronics" },
        new Category { Id = 7, Name = "Other" }
    );
  }
}
