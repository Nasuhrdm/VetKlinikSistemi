namespace VetKlinikSistemi.Models
{
    public class VetAvailability
    {
        public int Id { get; set; }

        public int VeterinarianId { get; set; }
        public Veterinarian? Veterinarian { get; set; }

        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}