using System.ComponentModel.DataAnnotations;

namespace VetKlinikSistemi.DTOs
{
    public class AppointmentCreateViewModel
    {
        [Required(ErrorMessage = "Evcil hayvan seçimi zorunludur.")]
        public int PetId { get; set; }

        [Required(ErrorMessage = "Hizmet seçimi zorunludur.")]
        public int ClinicServiceId { get; set; }

        [Required(ErrorMessage = "Veteriner hekim seçimi zorunludur.")]
        public int VeterinarianId { get; set; }

        [Required(ErrorMessage = "Randevu tarihi zorunludur.")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Lütfen uygun bir randevu saati seçiniz.")]
        public string SelectedTime { get; set; } = string.Empty;
    }
}
