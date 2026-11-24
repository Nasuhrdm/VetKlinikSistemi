using System.ComponentModel.DataAnnotations;

namespace VetKlinikSistemi.Models
{
    public class ClinicService
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(5, 480, ErrorMessage = "Süre 5 ile 480 dakika arasında olmalıdır.")]
        public int DurationMinutes { get; set; }

        [Range(0, 100000, ErrorMessage = "Geçerli bir ücret giriniz.")]
        public decimal Price { get; set; }

        public int ClinicId { get; set; }
        public Clinic? Clinic { get; set; }

        public ICollection<Veterinarian> Veterinarians { get; set; } = new List<Veterinarian>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}