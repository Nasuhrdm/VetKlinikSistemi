using System.ComponentModel.DataAnnotations;

namespace VetKlinikSistemi.DTOs
{
    public class PetFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Evcil hayvan adı zorunludur.")]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tür seçimi zorunludur.")]
        [StringLength(50)]
        public string Species { get; set; } = string.Empty;

        [Required(ErrorMessage = "Irk bilgisi zorunludur.")]
        [StringLength(50)]
        public string Breed { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; } = DateTime.Today.AddYears(-1);

        [Required(ErrorMessage = "Kilo bilgisi zorunludur.")]
        [Range(0.1, 500, ErrorMessage = "Kilo 0.1 ile 500 kg arasında olmalıdır.")]
        public decimal Weight { get; set; }

        public string? ExistingPhotoUrl { get; set; }

        public IFormFile? PhotoFile { get; set; }
    }

    public class VaccinationCreateViewModel
    {
        public int PetId { get; set; }

        [Required(ErrorMessage = "Aşı adı zorunludur.")]
        [StringLength(100)]
        public string VaccineName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Uygulama tarihi zorunludur.")]
        [DataType(DataType.Date)]
        public DateTime DateAdministered { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        public DateTime? NextDueDate { get; set; }

        [StringLength(300)]
        public string? Notes { get; set; }
    }
}
