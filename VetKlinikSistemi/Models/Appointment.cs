using System.ComponentModel.DataAnnotations;

namespace VetKlinikSistemi.Models
{
    public enum AppointmentStatus
    {
        Bekliyor = 0,
        Onaylandi = 1,
        Reddedildi = 2,
        Tamamlandi = 3
    }

    public class Appointment
    {
        public int Id { get; set; }

        public int PetId { get; set; }
        public Pet? Pet { get; set; }

        public int VeterinarianId { get; set; }
        public Veterinarian? Veterinarian { get; set; }

        public int ClinicServiceId { get; set; }
        public ClinicService? ClinicService { get; set; }

        public DateTime AppointmentDate { get; set; }

        public int DurationMinutes { get; set; }
        public decimal Price { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Bekliyor;

        [StringLength(1000)]
        public string? ExaminationNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}