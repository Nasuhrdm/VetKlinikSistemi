using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetKlinikSistemi.Data;
using VetKlinikSistemi.DTOs;
using VetKlinikSistemi.Models;

namespace VetKlinikSistemi.Controllers
{
    [Authorize(Roles = "Uye")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var appointments = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Veterinarian)
                .Include(a => a.ClinicService)
                .Where(a => a.Pet != null && a.Pet.OwnerId == userId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? petId = null)
        {
            var userId = _userManager.GetUserId(User);
            var pets = await _context.Pets
                .Where(p => p.OwnerId == userId)
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewBag.Pets = pets;
            ViewBag.Services = await _context.ClinicServices
                .OrderBy(s => s.Name)
                .ToListAsync();

            var model = new AppointmentCreateViewModel
            {
                PetId = petId ?? (pets.FirstOrDefault()?.Id ?? 0),
                AppointmentDate = DateTime.Today
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetVeterinariansByService(int serviceId)
        {
            var vets = await _context.Veterinarians
                .Where(v => v.Services.Any(s => s.Id == serviceId))
                .OrderBy(v => v.FullName)
                .Select(v => new
                {
                    id = v.Id,
                    fullName = v.FullName,
                    specialty = v.Specialty
                })
                .ToListAsync();

            return Json(vets);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int veterinarianId, int serviceId, DateTime date)
        {
            if (date.Date < DateTime.Today)
            {
                return Json(new { slots = Array.Empty<object>(), message = "Geçmiş bir tarih için randevu alınamaz." });
            }

            var service = await _context.ClinicServices.FindAsync(serviceId);
            if (service == null)
            {
                return Json(new { slots = Array.Empty<object>(), message = "Seçilen hizmet bulunamadı." });
            }

            var availabilities = await _context.VetAvailabilities
                .Where(a => a.VeterinarianId == veterinarianId && a.DayOfWeek == date.DayOfWeek)
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            if (!availabilities.Any())
            {
                return Json(new { slots = Array.Empty<object>(), message = "Seçilen veteriner hekim bu günde çalışmamaktadır." });
            }

            var existingAppointments = await _context.Appointments
                .Include(a => a.ClinicService)
                .Where(a => a.VeterinarianId == veterinarianId
                            && a.AppointmentDate.Date == date.Date
                            && a.Status != AppointmentStatus.Reddedildi)
                .ToListAsync();

            var duration = TimeSpan.FromMinutes(service.DurationMinutes > 0 ? service.DurationMinutes : 30);
            var nowTime = DateTime.Now.TimeOfDay;
            var slotList = new List<object>();

            foreach (var block in availabilities)
            {
                var current = block.StartTime;
                while (current + duration <= block.EndTime)
                {
                    var slotEnd = current + duration;
                    bool isPast = date.Date == DateTime.Today && current <= nowTime;

                    if (!isPast)
                    {
                        bool isBooked = existingAppointments.Any(a =>
                        {
                            var appStart = a.AppointmentDate.TimeOfDay;
                            int mins = a.DurationMinutes > 0 ? a.DurationMinutes : (a.ClinicService?.DurationMinutes ?? 30);
                            var appEnd = appStart + TimeSpan.FromMinutes(mins);
                            return current < appEnd && slotEnd > appStart;
                        });

                        slotList.Add(new
                        {
                            time = current.ToString(@"hh\:mm"),
                            endTime = slotEnd.ToString(@"hh\:mm"),
                            isAvailable = !isBooked
                        });
                    }

                    current = slotEnd;
                }
            }

            string msg = slotList.Any()
                ? string.Empty
                : "Seçilen tarih için uygun çalışma saati aralığı kalmamıştır.";

            return Json(new { slots = slotList, message = msg });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentCreateViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            if (!TimeSpan.TryParse(model.SelectedTime, out var selectedTime))
            {
                ModelState.AddModelError("SelectedTime", "Geçerli bir randevu saati seçiniz.");
            }

            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == model.PetId && p.OwnerId == userId);
            if (pet == null)
            {
                ModelState.AddModelError("PetId", "Seçilen evcil hayvan size ait değil.");
            }

            var service = await _context.ClinicServices.FindAsync(model.ClinicServiceId);
            if (service == null)
            {
                ModelState.AddModelError("ClinicServiceId", "Geçersiz hizmet seçimi.");
            }

            var vet = await _context.Veterinarians
                .Include(v => v.Services)
                .Include(v => v.Availabilities)
                .FirstOrDefaultAsync(v => v.Id == model.VeterinarianId);

            if (vet == null || (service != null && !vet.Services.Any(s => s.Id == service.Id)))
            {
                ModelState.AddModelError("VeterinarianId", "Seçilen veteriner hekim bu hizmeti vermemektedir.");
            }

            DateTime fullAppointmentDate = model.AppointmentDate.Date + selectedTime;
            if (fullAppointmentDate <= DateTime.Now)
            {
                ModelState.AddModelError("AppointmentDate", "Geçmiş bir tarih veya saate randevu alınamaz.");
            }

            if (ModelState.IsValid && vet != null && service != null)
            {
                int serviceMins = service.DurationMinutes > 0 ? service.DurationMinutes : 30;
                var duration = TimeSpan.FromMinutes(serviceMins);
                var slotEnd = selectedTime + duration;

                bool withinAvailability = vet.Availabilities.Any(a =>
                    a.DayOfWeek == model.AppointmentDate.DayOfWeek &&
                    selectedTime >= a.StartTime &&
                    slotEnd <= a.EndTime);

                if (!withinAvailability)
                {
                    ModelState.AddModelError("SelectedTime", "Seçilen saat veteriner hekimin çalışma saatleri dışındadır.");
                }

                var vetAppointments = await _context.Appointments
                    .Include(a => a.ClinicService)
                    .Where(a => a.VeterinarianId == vet.Id
                                && a.AppointmentDate.Date == model.AppointmentDate.Date
                                && a.Status != AppointmentStatus.Reddedildi)
                    .ToListAsync();

                bool vetConflict = vetAppointments.Any(a =>
                {
                    var appStart = a.AppointmentDate.TimeOfDay;
                    int mins = a.DurationMinutes > 0 ? a.DurationMinutes : (a.ClinicService?.DurationMinutes ?? 30);
                    var appEnd = appStart + TimeSpan.FromMinutes(mins);
                    return selectedTime < appEnd && slotEnd > appStart;
                });

                if (vetConflict)
                {
                    ModelState.AddModelError("SelectedTime", "Seçilen saat dilimi az önce başka bir hasta tarafından alındı. Lütfen başka bir saat seçiniz.");
                }

                var petAppointments = await _context.Appointments
                    .Include(a => a.ClinicService)
                    .Where(a => a.PetId == model.PetId
                                && a.AppointmentDate.Date == model.AppointmentDate.Date
                                && a.Status != AppointmentStatus.Reddedildi)
                    .ToListAsync();

                bool petConflict = petAppointments.Any(a =>
                {
                    var appStart = a.AppointmentDate.TimeOfDay;
                    int mins = a.DurationMinutes > 0 ? a.DurationMinutes : (a.ClinicService?.DurationMinutes ?? 30);
                    var appEnd = appStart + TimeSpan.FromMinutes(mins);
                    return selectedTime < appEnd && slotEnd > appStart;
                });

                if (petConflict)
                {
                    ModelState.AddModelError("SelectedTime", "Bu evcil hayvanın seçilen saat aralığında zaten başka bir randevusu bulunuyor.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Pets = await _context.Pets.Where(p => p.OwnerId == userId).OrderBy(p => p.Name).ToListAsync();
                ViewBag.Services = await _context.ClinicServices.OrderBy(s => s.Name).ToListAsync();
                return View(model);
            }

            var appointment = new Appointment
            {
                PetId = model.PetId,
                VeterinarianId = model.VeterinarianId,
                ClinicServiceId = model.ClinicServiceId,
                AppointmentDate = fullAppointmentDate,
                DurationMinutes = service!.DurationMinutes > 0 ? service.DurationMinutes : 30,
                Price = service.Price,
                Status = AppointmentStatus.Bekliyor
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{pet!.Name} için {fullAppointmentDate:dd.MM.yyyy HH:mm} tarihli randevu talebiniz oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User);
            var appointment = await _context.Appointments
                .Include(a => a.Pet)
                .FirstOrDefaultAsync(a => a.Id == id && a.Pet != null && a.Pet.OwnerId == userId);

            if (appointment == null)
                return NotFound();

            if (appointment.Status == AppointmentStatus.Tamamlandi || appointment.Status == AppointmentStatus.Reddedildi)
            {
                TempData["Error"] = "Bu randevu iptal edilemez.";
                return RedirectToAction(nameof(Index));
            }

            appointment.Status = AppointmentStatus.Reddedildi;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Randevunuz iptal edildi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
