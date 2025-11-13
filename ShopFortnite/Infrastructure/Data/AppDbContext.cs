using Microsoft.EntityFrameworkCore;
using ShopFortnite.Domain.Entities;

namespace ShopFortnite.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Cosmetic> Cosmetics => Set<Cosmetic>();
    public DbSet<UserCosmetic> UserCosmetics => Set<UserCosmetic>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Vbucks).HasColumnType("decimal(18,2)");
        });

        // Cosmetic configuration
        modelBuilder.Entity<Cosmetic>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ExternalId).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Type).HasMaxLength(100);
            entity.Property(e => e.Rarity).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
        });

        // UserCosmetic configuration (many-to-many with extra fields)
        modelBuilder.Entity<UserCosmetic>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.CosmeticId });

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserCosmetics)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Cosmetic)
                .WithMany(c => c.UserCosmetics)
                .HasForeignKey(e => e.CosmeticId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.PriceAtPurchase).HasColumnType("decimal(18,2)");
        });

        // Transaction configuration
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Cosmetic)
                .WithMany(c => c.Transactions)
                .HasForeignKey(e => e.CosmeticId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Type).HasConversion<string>();
        });
    }
}
