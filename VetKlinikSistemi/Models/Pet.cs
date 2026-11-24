using System.ComponentModel.DataAnnotations;

namespace VetKlinikSistemi.Models
{
    public class Pet
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Evcil hayvan adı zorunludur.")]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tür zorunludur.")]
        [StringLength(50)]
        public string Species { get; set; } = string.Empty;

        [Required(ErrorMessage = "Irk zorunludur.")]
        [StringLength(50)]
        public string Breed { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Range(0.1, 500, ErrorMessage = "Geçerli bir kilo giriniz.")]
        public decimal Weight { get; set; }

        [StringLength(300)]
        public string? PhotoUrl { get; set; }

        [Required]
        public string OwnerId { get; set; } = string.Empty;

        public ICollection<VaccinationRecord> VaccinationRecords { get; set; } = new List<VaccinationRecord>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}