using Inventory_Management_Platform.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Management_Platform.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<InventoryTag> InventoryTags => Set<InventoryTag>();
    public DbSet<InventoryAccess> InventoryAccesses => Set<InventoryAccess>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Like> Likes => Set<Like>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── InventoryAccess: composite PK ─────────────────────────────────────
        modelBuilder.Entity<InventoryAccess>()
            .HasKey(ia => new { ia.InventoryId, ia.UserId });

        modelBuilder.Entity<InventoryAccess>()
            .HasOne(ia => ia.Inventory)
            .WithMany(i => i.AccessList)
            .HasForeignKey(ia => ia.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InventoryAccess>()
            .HasOne(ia => ia.User)
            .WithMany(u => u.InventoryAccesses)
            .HasForeignKey(ia => ia.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Like: composite PK ────────────────────────────────────────────────
        modelBuilder.Entity<Like>()
            .HasKey(l => new { l.ItemId, l.UserId });

        modelBuilder.Entity<Like>()
            .HasOne(l => l.Item)
            .WithMany(i => i.Likes)
            .HasForeignKey(l => l.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Like>()
            .HasOne(l => l.User)
            .WithMany(u => u.Likes)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Inventory ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Inventory>()
            .HasOne(i => i.Owner)
            .WithMany(u => u.OwnedInventories)
            .HasForeignKey(i => i.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Inventory>()
            .HasOne(i => i.Category)
            .WithMany(c => c.Inventories)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // ── InventoryTag: composite PK ────────────────────────────────────────
        modelBuilder.Entity<InventoryTag>()
            .HasKey(it => new { it.InventoryId, it.TagId });

        modelBuilder.Entity<InventoryTag>()
            .HasOne(it => it.Inventory)
            .WithMany()
            .HasForeignKey(it => it.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InventoryTag>()
            .HasOne(it => it.Tag)
            .WithMany()
            .HasForeignKey(it => it.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        // Inventory ↔ Tag: skip navigation wired to the join entity above.
        // UsingEntity<InventoryTag>() tells EF Core to use the already-configured entity
        // as the join table — enables inventory.Tags.Add(tag) without touching InventoryTag directly.
        modelBuilder.Entity<Inventory>()
            .HasMany(i => i.Tags)
            .WithMany(t => t.Inventories)
            .UsingEntity<InventoryTag>();

        // CustomIdFormat stored as JSONB
        modelBuilder.Entity<Inventory>()
            .Property(i => i.CustomIdFormat)
            .HasColumnType("jsonb");

        // Optimistic concurrency: xmin is a Postgres system column that changes on every UPDATE.
        // Mapped as a shadow property — no Version field needed on the entity.
        modelBuilder.Entity<Inventory>()
            .Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        // Full-text search: computed tsvector column over Title + DescriptionMd
        modelBuilder.Entity<Inventory>()
            .HasGeneratedTsVectorColumn(
                i => i.SearchVector,
                "english",
                i => new { i.Title, i.DescriptionMd })
            .HasIndex(i => i.SearchVector)
            .HasMethod("GIN");

        // ── Item ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<Item>()
            .HasOne(i => i.Inventory)
            .WithMany(inv => inv.Items)
            .HasForeignKey(i => i.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Item>()
            .HasOne(i => i.CreatedBy)
            .WithMany(u => u.CreatedItems)
            .HasForeignKey(i => i.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        // Composite unique index: one CustomId per Inventory
        modelBuilder.Entity<Item>()
            .HasIndex(i => new { i.InventoryId, i.CustomId })
            .IsUnique();

        // Optimistic concurrency: xmin shadow property — same pattern as Inventory.
        modelBuilder.Entity<Item>()
            .Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        // ── Post ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<Post>()
            .HasOne(p => p.Inventory)
            .WithMany(i => i.Posts)
            .HasForeignKey(p => p.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Post>()
            .HasOne(p => p.Author)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Tag: unique name + btree index for prefix autocomplete ────────────
        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.Name)
            .IsUnique();

        // ── Category seed data ────────────────────────────────────────────────
        modelBuilder.Entity<Category>().HasData(
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