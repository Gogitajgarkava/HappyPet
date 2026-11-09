using HappyPet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HappyPet.Infrastructure.Persistence
{
    public class HappyPetDbContext : DbContext
    {
        public HappyPetDbContext(DbContextOptions<HappyPetDbContext> options) : base(options) { }
               public DbSet<User> Users => Set<User>();
        public DbSet<Pet> Pets => Set<Pet>();
        public DbSet<PetPhoto> PetPhotos => Set<PetPhoto>();
        public DbSet<Vaccination> Vaccinations => Set<Vaccination>();
        public DbSet<UserOtpCode> UserOtpCodes => Set<UserOtpCode>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.PersonalId).IsUnique();
                entity.Property(u => u.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(u => u.LastName).IsRequired().HasMaxLength(50);
                entity.Property(u => u.PersonalId).IsRequired().HasMaxLength(11);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Password).IsRequired();
                entity.Property(u => u.PhoneNumber).IsRequired().HasMaxLength(20);
            });

            modelBuilder.Entity<Pet>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.MicrochipNumber).IsUnique();
                entity.Property(p => p.Name).IsRequired().HasMaxLength(50);
                entity.Property(p => p.MicrochipNumber).IsRequired().HasMaxLength(15);
                entity.Property(p => p.Breed).HasMaxLength(50);
                entity.Property(p => p.LastSeenAddress).HasMaxLength(300);
                entity.Property(p => p.ContactPhone).HasMaxLength(20);
                entity.HasIndex(p => new { p.IsLost, p.LostAt });

                entity.HasOne(p => p.Owner)
                      .WithMany(u => u.Pets)
                      .HasForeignKey(p => p.OwnerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PetPhoto>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.PetId);
                entity.Property(p => p.Key).IsRequired().HasMaxLength(500);
                entity.Property(p => p.ContentType).IsRequired().HasMaxLength(100);

                entity.HasOne(p => p.Pet)
                      .WithMany(p => p.Photos)
                      .HasForeignKey(p => p.PetId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Vaccination>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.Property(v => v.VaccineName).IsRequired().HasMaxLength(100);
                entity.Property(v => v.Disease).HasMaxLength(100);
                entity.Property(v => v.Manufacturer).HasMaxLength(100);
                entity.Property(v => v.BatchNumber).HasMaxLength(50);
                entity.Property(v => v.CertificateNumber).HasMaxLength(50);
                entity.Property(v => v.VeterinarianName).HasMaxLength(100);
                entity.Property(v => v.ClinicName).HasMaxLength(100);
                entity.Property(v => v.Notes).HasMaxLength(500);
                entity.Property(v => v.AirsVaccinationId).HasMaxLength(100);
                entity.HasIndex(v => new { v.PetId, v.VaccinatedAt });
                entity.HasIndex(v => new { v.PetId, v.AirsVaccinationId }).IsUnique();

                entity.HasOne(v => v.Pet)
                    .WithMany(p => p.Vaccinations)
                    .HasForeignKey(v => v.PetId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserOtpCode>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => new { c.UserId, c.Purpose, c.CreatedAt });
                entity.Property(c => c.Purpose)
                    .HasConversion<string>()
                    .HasMaxLength(32)
                    .IsRequired();
                entity.Property(c => c.CodeHash).IsRequired().HasMaxLength(64);
                entity.Property(c => c.ExpiresAt).IsRequired();

                entity.HasOne(c => c.User)
                    .WithMany(u => u.OtpCodes)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasIndex(t => t.TokenHash).IsUnique();
                entity.HasIndex(t => new { t.UserId, t.ExpiresAt });
                entity.HasIndex(t => t.FamilyId);
                entity.Property(t => t.TokenHash).IsRequired().HasMaxLength(64);
                entity.Property(t => t.ReplacedByTokenHash).HasMaxLength(64);
                entity.Property(t => t.RevocationReason).HasMaxLength(50);
                entity.Property(t => t.ExpiresAt).IsRequired();

                entity.HasOne(t => t.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
