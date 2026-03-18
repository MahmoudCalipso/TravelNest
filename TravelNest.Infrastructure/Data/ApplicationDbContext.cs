using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TravelNest.Domain.Entities;

namespace TravelNest.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<PropertyMedia> PropertyMedia => Set<PropertyMedia>();
    public DbSet<PropertyAmenity> PropertyAmenities => Set<PropertyAmenity>();
    public DbSet<PropertyAvailability> PropertyAvailabilities => Set<PropertyAvailability>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<UserMedia> UserMedia => Set<UserMedia>();
    public DbSet<MediaLike> MediaLikes => Set<MediaLike>();
    public DbSet<MediaComment> MediaComments => Set<MediaComment>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ==================== USER ====================
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Role).HasConversion<string>();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ==================== PROPERTY ====================
        modelBuilder.Entity<Property>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.PricePerNight).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasOne(e => e.Provider)
                .WithMany(u => u.Properties)
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.Country, e.City });
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.PricePerNight);
            entity.HasIndex(e => e.IsAvailable);
            entity.HasIndex(e => e.IsApproved);
        });

        // ==================== PROPERTY MEDIA ====================
        modelBuilder.Entity<PropertyMedia>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasOne(e => e.Property)
                .WithMany(p => p.Media)
                .HasForeignKey(e => e.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ==================== PROPERTY AMENITY ====================
        modelBuilder.Entity<PropertyAmenity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasOne(e => e.Property)
                .WithMany(p => p.Amenities)
                .HasForeignKey(e => e.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ==================== PROPERTY AVAILABILITY ====================
        modelBuilder.Entity<PropertyAvailability>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SpecialPrice).HasPrecision(18, 2);
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasOne(e => e.Property)
                .WithMany(p => p.Availabilities)
                .HasForeignKey(e => e.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ==================== BOOKING ====================
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PricePerNight).HasPrecision(18, 2);
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.BookingReference).HasMaxLength(20);
            entity.HasIndex(e => e.BookingReference).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasOne(e => e.Traveller)
                .WithMany(u => u.Bookings)
                .HasForeignKey(e => e.TravellerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Property)
                .WithMany(p => p.Bookings)
                .HasForeignKey(e => e.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ==================== PAYMENT ====================
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.Method).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasOne(e => e.Booking)
                .WithOne(b => b.Payment)
                .HasForeignKey<Payment>(e => e.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ==================== REVIEW ====================
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasOne(e => e.Traveller)
                .WithMany(u => u.Reviews)
                .HasForeignKey(e => e.TravellerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Property)
                .WithMany(p => p.Reviews)
                .HasForeignKey(e => e.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TravellerId, e.PropertyId }).IsUnique();
        });

        // ==================== USER MEDIA ====================
        modelBuilder.Entity<UserMedia>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasOne(e => e.User)
                .WithMany(u => u.MediaPosts)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Property)
                .WithMany()
                .HasForeignKey(e => e.PropertyId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ==================== MEDIA LIKE ====================
        modelBuilder.Entity<MediaLike>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.UserMediaId }).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasOne(e => e.User)
                .WithMany(u => u.MediaLikes)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UserMedia)
                .WithMany(m => m.Likes)
                .HasForeignKey(e => e.UserMediaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ==================== MEDIA COMMENT ====================
        modelBuilder.Entity<MediaComment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).HasMaxLength(1000);
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasOne(e => e.User)
                .WithMany(u => u.MediaComments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UserMedia)
                .WithMany(m => m.Comments)
                .HasForeignKey(e => e.UserMediaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ==================== FAVORITE ====================
        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.PropertyId }).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Property)
                .WithMany(p => p.Favorites)
                .HasForeignKey(e => e.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ==================== CONTACT MESSAGE ====================
        modelBuilder.Entity<ContactMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Subject).HasMaxLength(200);
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasOne(e => e.Sender)
                .WithMany()
                .HasForeignKey(e => e.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Receiver)
                .WithMany()
                .HasForeignKey(e => e.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Booking)
                .WithMany()
                .HasForeignKey(e => e.BookingId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ==================== SEED SUPER ADMIN ====================
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            FirstName = "Super",
            LastName = "Admin",
            Email = "admin@travelnest.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123456"),
            Role = Domain.Enums.UserRole.SuperAdmin,
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
