using System.ComponentModel.DataAnnotations;

namespace VetKlinikSistemi.Models
{
    public class Veterinarian
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
        [StringLength(150)]
        public string Specialty { get; set; } = string.Empty;

        public string? UserId { get; set; }

        public int ClinicId { get; set; }
        public Clinic? Clinic { get; set; }

        public ICollection<ClinicService> Services { get; set; } = new List<ClinicService>();
        public ICollection<VetAvailability> Availabilities { get; set; } = new List<VetAvailability>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}