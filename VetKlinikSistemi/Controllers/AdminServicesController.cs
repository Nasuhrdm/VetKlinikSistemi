using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetKlinikSistemi.Data;
using VetKlinikSistemi.Models;

namespace VetKlinikSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var services = await _context.ClinicServices
                .Include(s => s.Veterinarians)
                .OrderBy(s => s.Name)
                .ToListAsync();
            return View(services);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ClinicService { DurationMinutes = 30 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClinicService model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var clinic = await _context.Clinics.FirstOrDefaultAsync();
            if (clinic == null)
                return NotFound();

            model.ClinicId = clinic.Id;
            _context.ClinicServices.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Yeni hizmet başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _context.ClinicServices.FindAsync(id);
            if (service == null)
                return NotFound();

            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClinicService model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var service = await _context.ClinicServices.FindAsync(model.Id);
            if (service == null)
                return NotFound();

            service.Name = model.Name;
            service.Description = model.Description;
            service.DurationMinutes = model.DurationMinutes;
            service.Price = model.Price;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Hizmet bilgileri güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var hasAppointments = await _context.Appointments.AnyAsync(a => a.ClinicServiceId == id);
            if (hasAppointments)
            {
                TempData["Error"] = "Bu hizmete kayıtlı randevular bulunduğu için silinemez.";
                return RedirectToAction(nameof(Index));
            }

            var service = await _context.ClinicServices
                .Include(s => s.Veterinarians)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service != null)
            {
                service.Veterinarians.Clear();
                _context.ClinicServices.Remove(service);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Hizmet silindi.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
