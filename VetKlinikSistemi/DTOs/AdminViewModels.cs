using System.ComponentModel.DataAnnotations;

namespace VetKlinikSistemi.DTOs
{
    public class VeterinarianCreateViewModel
    {
        [Required(ErrorMessage = "Hekim adı soyadı zorunludur.")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
        [StringLength(150)]
        public string Specialty { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giriş şifresi zorunludur.")]
        [DataType(DataType.Password)]
        [MinLength(3, ErrorMessage = "Şifre en az 3 karakter olmalıdır.")]
        public string Password { get; set; } = string.Empty;

        public List<int> SelectedServiceIds { get; set; } = new List<int>();
    }

    public class VeterinarianEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hekim adı soyadı zorunludur.")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
        [StringLength(150)]
        public string Specialty { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public List<int> SelectedServiceIds { get; set; } = new List<int>();
    }

    public class VetAvailabilityCreateViewModel
    {
        public int VeterinarianId { get; set; }

        [Required(ErrorMessage = "Gün seçimi zorunludur.")]
        public DayOfWeek DayOfWeek { get; set; }

        [Required(ErrorMessage = "Başlangıç saati zorunludur.")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Bitiş saati zorunludur.")]
        public TimeSpan EndTime { get; set; }
    }
}
