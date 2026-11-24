using System.ComponentModel.DataAnnotations;

namespace VetKlinikSistemi.Models
{
    public class VaccinationRecord
    {
        public int Id { get; set; }

        public int PetId { get; set; }
        public Pet? Pet { get; set; }

        [Required(ErrorMessage = "Aşı adı zorunludur.")]
        [StringLength(100)]
        public string VaccineName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime DateAdministered { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NextDueDate { get; set; }

        [StringLength(300)]
        public string? Notes { get; set; }
    }
}