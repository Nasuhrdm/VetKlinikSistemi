using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetKlinikSistemi.Data;
using VetKlinikSistemi.DTOs;
using VetKlinikSistemi.Models;

namespace VetKlinikSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminVeterinariansController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminVeterinariansController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vets = await _context.Veterinarians
                .Include(v => v.Services)
                .Include(v => v.Availabilities)
                .OrderBy(v => v.FullName)
                .ToListAsync();
            return View(vets);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Services = await _context.ClinicServices.OrderBy(s => s.Name).ToListAsync();
            return View(new VeterinarianCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VeterinarianCreateViewModel model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten sistemde kayıtlı.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Services = await _context.ClinicServices.OrderBy(s => s.Name).ToListAsync();
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true
            };

            var userResult = await _userManager.CreateAsync(user, model.Password);
            if (!userResult.Succeeded)
            {
                foreach (var error in userResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                ViewBag.Services = await _context.ClinicServices.OrderBy(s => s.Name).ToListAsync();
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Veteriner");

            var clinic = await _context.Clinics.FirstOrDefaultAsync();
            var selectedServices = await _context.ClinicServices
                .Where(s => model.SelectedServiceIds.Contains(s.Id))
                .ToListAsync();

            var vet = new Veterinarian
            {
                FullName = model.FullName,
                Specialty = model.Specialty,
                UserId = user.Id,
                ClinicId = clinic!.Id,
                Services = selectedServices
            };

            _context.Veterinarians.Add(vet);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Veteriner hekim ve kullanıcı hesabı başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vet = await _context.Veterinarians
                .Include(v => v.Services)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vet == null)
                return NotFound();

            var user = vet.UserId != null ? await _userManager.FindByIdAsync(vet.UserId) : null;

            var model = new VeterinarianEditViewModel
            {
                Id = vet.Id,
                FullName = vet.FullName,
                Specialty = vet.Specialty,
                Email = user?.Email ?? "-",
                SelectedServiceIds = vet.Services.Select(s => s.Id).ToList()
            };

            ViewBag.Services = await _context.ClinicServices.OrderBy(s => s.Name).ToListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VeterinarianEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Services = await _context.ClinicServices.OrderBy(s => s.Name).ToListAsync();
                return View(model);
            }

            var vet = await _context.Veterinarians
                .Include(v => v.Services)
                .FirstOrDefaultAsync(v => v.Id == model.Id);

            if (vet == null)
                return NotFound();

            vet.FullName = model.FullName;
            vet.Specialty = model.Specialty;

            vet.Services.Clear();
            var selectedServices = await _context.ClinicServices
                .Where(s => model.SelectedServiceIds.Contains(s.Id))
                .ToListAsync();
            foreach (var service in selectedServices)
            {
                vet.Services.Add(service);
            }

            if (vet.UserId != null)
            {
                var user = await _userManager.FindByIdAsync(vet.UserId);
                if (user != null)
                {
                    user.FullName = model.FullName;
                    await _userManager.UpdateAsync(user);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Veteriner hekim bilgileri güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var hasAppointments = await _context.Appointments.AnyAsync(a => a.VeterinarianId == id);
            if (hasAppointments)
            {
                TempData["Error"] = "Bu hekime ait randevu kayıtları bulunduğu için silinemez.";
                return RedirectToAction(nameof(Index));
            }

            var vet = await _context.Veterinarians
                .Include(v => v.Services)
                .Include(v => v.Availabilities)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vet != null)
            {
                var userId = vet.UserId;
                vet.Services.Clear();
                _context.VetAvailabilities.RemoveRange(vet.Availabilities);
                _context.Veterinarians.Remove(vet);
                await _context.SaveChangesAsync();

                if (userId != null)
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                        await _userManager.DeleteAsync(user);
                }

                TempData["Success"] = "Veteriner hekim kaydı silindi.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Availabilities(int id)
        {
            var vet = await _context.Veterinarians
                .Include(v => v.Availabilities)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vet == null)
                return NotFound();

            ViewBag.Veterinarian = vet;
            return View(new VetAvailabilityCreateViewModel
            {
                VeterinarianId = id,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(17, 0, 0)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAvailability(VetAvailabilityCreateViewModel model)
        {
            var vet = await _context.Veterinarians
                .Include(v => v.Clinic)
                .Include(v => v.Availabilities)
                .FirstOrDefaultAsync(v => v.Id == model.VeterinarianId);

            if (vet == null)
                return NotFound();

            if (model.StartTime >= model.EndTime)
            {
                TempData["Error"] = "Bitiş saati başlangıç saatinden sonra olmalıdır.";
                return RedirectToAction(nameof(Availabilities), new { id = model.VeterinarianId });
            }

            if (vet.Clinic != null && (model.StartTime < vet.Clinic.OpeningTime || model.EndTime > vet.Clinic.ClosingTime))
            {
                TempData["Error"] = "Çalışma saatleri klinik mesai saatleri (" +
                    vet.Clinic.OpeningTime.ToString(@"hh\:mm") + " - " +
                    vet.Clinic.ClosingTime.ToString(@"hh\:mm") + ") dışında olamaz.";
                return RedirectToAction(nameof(Availabilities), new { id = model.VeterinarianId });
            }

            bool overlaps = vet.Availabilities.Any(a =>
                a.DayOfWeek == model.DayOfWeek &&
                model.StartTime < a.EndTime &&
                model.EndTime > a.StartTime);

            if (overlaps)
            {
                TempData["Error"] = "Seçilen gün için mevcut çalışma saatleriyle çakışma var.";
                return RedirectToAction(nameof(Availabilities), new { id = model.VeterinarianId });
            }

            _context.VetAvailabilities.Add(new VetAvailability
            {
                VeterinarianId = model.VeterinarianId,
                DayOfWeek = model.DayOfWeek,
                StartTime = model.StartTime,
                EndTime = model.EndTime
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Müsaitlik saati eklendi.";
            return RedirectToAction(nameof(Availabilities), new { id = model.VeterinarianId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAvailability(int id)
        {
            var slot = await _context.VetAvailabilities.FindAsync(id);
            if (slot == null)
                return NotFound();

            int vetId = slot.VeterinarianId;
            _context.VetAvailabilities.Remove(slot);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Müsaitlik kaydı kaldırıldı.";
            return RedirectToAction(nameof(Availabilities), new { id = vetId });
        }
    }
}
