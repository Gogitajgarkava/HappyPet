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
        public DbSet<MarketplaceListing> MarketplaceListings => Set<MarketplaceListing>();
        public DbSet<MarketplaceListingPhoto> MarketplaceListingPhotos => Set<MarketplaceListingPhoto>();
        public DbSet<OwnershipTransfer> OwnershipTransfers => Set<OwnershipTransfer>();
        public DbSet<VaccinationReminder> VaccinationReminders => Set<VaccinationReminder>();
        public DbSet<ShelterProfile> ShelterProfiles => Set<ShelterProfile>();
        public DbSet<AdoptionListing> AdoptionListings => Set<AdoptionListing>();
        public DbSet<AdoptionApplication> AdoptionApplications => Set<AdoptionApplication>();

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
                entity.HasIndex(p => p.MicrochipNumber)
                    .IsUnique()
                    .HasFilter("\"MicrochipNumber\" <> ''");
                entity.Property(p => p.Name).IsRequired().HasMaxLength(50);
                entity.Property(p => p.MicrochipNumber).IsRequired().HasMaxLength(15);
                entity.Property(p => p.Breed).HasMaxLength(50);
                entity.Property(p => p.LastSeenAddress).HasMaxLength(300);
                entity.Property(p => p.ContactPhone).HasMaxLength(20);
                entity.Property(p => p.DeathCause).HasMaxLength(250);
                entity.Property(p => p.PublicProfileToken).HasMaxLength(64);
                entity.HasIndex(p => p.PublicProfileToken).IsUnique();
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
                entity.Property(v => v.ManufacturerCountry).HasMaxLength(100);
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

            modelBuilder.Entity<VaccinationReminder>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.HasIndex(r => new { r.VaccinationId, r.DueAt, r.OffsetDays, r.Channel }).IsUnique();
                entity.HasIndex(r => new { r.OwnerUserId, r.CreatedAt });
                entity.HasIndex(r => new { r.Status, r.ScheduledFor });
                entity.Property(r => r.PetName).IsRequired().HasMaxLength(50);
                entity.Property(r => r.VaccineName).IsRequired().HasMaxLength(100);
                entity.Property(r => r.RecipientEmail).IsRequired().HasMaxLength(100);
                entity.Property(r => r.LastError).HasMaxLength(500);
                entity.Property(r => r.SkipReason).HasMaxLength(100);

                entity.HasOne(r => r.Vaccination)
                    .WithMany()
                    .HasForeignKey(r => r.VaccinationId)
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

            modelBuilder.Entity<MarketplaceListing>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.HasIndex(l => new { l.Status, l.ExpiresAt });
                entity.HasIndex(l => new { l.Species, l.ListingType });
                entity.HasIndex(l => l.SellerId);
                entity.HasIndex(l => l.City);
                entity.Property(l => l.PetName).IsRequired().HasMaxLength(50);
                entity.Property(l => l.Breed).HasMaxLength(50);
                entity.Property(l => l.Color).HasMaxLength(50);
                entity.Property(l => l.MicrochipNumber).HasMaxLength(15);
                entity.Property(l => l.City).IsRequired().HasMaxLength(100);
                entity.Property(l => l.ContactPhone).IsRequired().HasMaxLength(20);
                entity.Property(l => l.Description).HasMaxLength(2000);
                entity.Property(l => l.BlockReason).HasMaxLength(500);
                entity.Property(l => l.Price).HasPrecision(18, 2);

                entity.HasOne(l => l.Seller)
                    .WithMany()
                    .HasForeignKey(l => l.SellerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(l => l.Pet)
                    .WithMany()
                    .HasForeignKey(l => l.PetId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<MarketplaceListingPhoto>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.ListingId);
                entity.Property(p => p.Key).IsRequired().HasMaxLength(500);
                entity.Property(p => p.ContentType).IsRequired().HasMaxLength(100);

                entity.HasOne(p => p.Listing)
                    .WithMany(l => l.Photos)
                    .HasForeignKey(p => p.ListingId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OwnershipTransfer>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasIndex(t => t.ListingId).IsUnique();
                entity.HasIndex(t => new { t.Status, t.TransferredAt });
                entity.Property(t => t.MicrochipNumber).HasMaxLength(15);
                entity.Property(t => t.BuyerFullName).IsRequired().HasMaxLength(100);
                entity.Property(t => t.BuyerPersonalId).IsRequired().HasMaxLength(11);
                entity.Property(t => t.BuyerPhone).HasMaxLength(20);
                entity.Property(t => t.BuyerAddress).HasMaxLength(300);
                entity.Property(t => t.AirsReference).HasMaxLength(100);
                entity.Property(t => t.RejectionReason).HasMaxLength(500);
                entity.Property(t => t.OperatorNotes).HasMaxLength(500);
                entity.Property(t => t.Notes).HasMaxLength(500);
                entity.Property(t => t.Price).HasPrecision(18, 2);
                entity.HasIndex(t => new { t.Status, t.ProcessedAt });

                entity.HasOne(t => t.Listing)
                    .WithOne(l => l.OwnershipTransfer)
                    .HasForeignKey<OwnershipTransfer>(t => t.ListingId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ShelterProfile>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasIndex(s => s.UserId).IsUnique();
                entity.HasIndex(s => s.City);
                entity.HasIndex(s => s.IsVerified);
                entity.HasIndex(s => s.RegistrationNumber)
                    .IsUnique()
                    .HasFilter("\"RegistrationNumber\" IS NOT NULL");
                entity.Property(s => s.OrganizationName).IsRequired().HasMaxLength(200);
                entity.Property(s => s.RegistrationNumber).HasMaxLength(50);
                entity.Property(s => s.Description).HasMaxLength(2000);
                entity.Property(s => s.City).IsRequired().HasMaxLength(100);
                entity.Property(s => s.Address).HasMaxLength(300);
                entity.Property(s => s.PhoneNumber).IsRequired().HasMaxLength(20);
                entity.Property(s => s.Email).HasMaxLength(100);
                entity.Property(s => s.Website).HasMaxLength(200);

                entity.HasOne(s => s.User)
                    .WithOne(u => u.ShelterProfile)
                    .HasForeignKey<ShelterProfile>(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AdoptionListing>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.HasIndex(l => new { l.Status, l.City });
                entity.HasIndex(l => l.ShelterId);
                entity.HasIndex(l => l.PetId);
                entity.HasIndex(l => l.PublishedAt);
                entity.Property(l => l.Description).HasMaxLength(2000);
                entity.Property(l => l.City).IsRequired().HasMaxLength(100);
                entity.Property(l => l.AdoptionFee).HasPrecision(18, 2);
                entity.Property(l => l.PublishedAt).IsRequired();

                entity.HasOne(l => l.Shelter)
                    .WithMany()
                    .HasForeignKey(l => l.ShelterId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(l => l.Pet)
                    .WithMany()
                    .HasForeignKey(l => l.PetId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AdoptionApplication>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.HasIndex(a => new { a.ListingId, a.ApplicantUserId }).IsUnique();
                entity.HasIndex(a => new { a.ListingId, a.Status });
                entity.HasIndex(a => new { a.ApplicantUserId, a.CreatedAt });
                entity.Property(a => a.FullName).IsRequired().HasMaxLength(100);
                entity.Property(a => a.Email).IsRequired().HasMaxLength(100);
                entity.Property(a => a.Phone).IsRequired().HasMaxLength(20);
                entity.Property(a => a.Address).HasMaxLength(300);
                entity.Property(a => a.Message).HasMaxLength(2000);
                entity.Property(a => a.DecisionNote).HasMaxLength(500);

                entity.HasOne(a => a.Listing)
                    .WithMany(l => l.Applications)
                    .HasForeignKey(a => a.ListingId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Applicant)
                    .WithMany()
                    .HasForeignKey(a => a.ApplicantUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
