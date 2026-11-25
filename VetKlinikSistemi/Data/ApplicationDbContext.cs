using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VetKlinikSistemi.Models;

namespace VetKlinikSistemi.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Clinic> Clinics => Set<Clinic>();
        public DbSet<ClinicService> ClinicServices => Set<ClinicService>();
        public DbSet<Veterinarian> Veterinarians => Set<Veterinarian>();
        public DbSet<VetAvailability> VetAvailabilities => Set<VetAvailability>();
        public DbSet<Pet> Pets => Set<Pet>();
        public DbSet<VaccinationRecord> VaccinationRecords => Set<VaccinationRecord>();
        public DbSet<Appointment> Appointments => Set<Appointment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ClinicService>()
                .Property(s => s.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Pet>()
                .Property(p => p.Weight)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Appointment>()
                .Property(a => a.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ClinicService>()
                .HasOne(s => s.Clinic)
                .WithMany(c => c.Services)
                .HasForeignKey(s => s.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Veterinarian>()
                .HasOne(v => v.Clinic)
                .WithMany(c => c.Veterinarians)
                .HasForeignKey(v => v.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Pet)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PetId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Veterinarian)
                .WithMany(v => v.Appointments)
                .HasForeignKey(a => a.VeterinarianId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.ClinicService)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.ClinicServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
