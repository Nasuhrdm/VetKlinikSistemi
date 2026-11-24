using System.ComponentModel.DataAnnotations;

namespace VetKlinikSistemi.Models
{
    public class Clinic
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Klinik adı zorunludur.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adres bilgisi zorunludur.")]
        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        public TimeSpan OpeningTime { get; set; } = new TimeSpan(9, 0, 0);
        public TimeSpan ClosingTime { get; set; } = new TimeSpan(18, 0, 0);

        public ICollection<ClinicService> Services { get; set; } = new List<ClinicService>();
        public ICollection<Veterinarian> Veterinarians { get; set; } = new List<Veterinarian>();
    }
}