using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetKlinikSistemi.Data;
using VetKlinikSistemi.Models;

namespace VetKlinikSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminClinicController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminClinicController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var clinic = await _context.Clinics.FirstOrDefaultAsync();
            if (clinic == null)
                return NotFound();

            return View(clinic);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Clinic model)
        {
            if (model.OpeningTime >= model.ClosingTime)
            {
                ModelState.AddModelError("ClosingTime", "Kapanış saati açılış saatinden sonra olmalıdır.");
            }

            if (!ModelState.IsValid)
                return View(model);

            var clinic = await _context.Clinics.FindAsync(model.Id);
            if (clinic == null)
                return NotFound();

            clinic.Name = model.Name;
            clinic.Address = model.Address;
            clinic.Phone = model.Phone;
            clinic.OpeningTime = model.OpeningTime;
            clinic.ClosingTime = model.ClosingTime;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Klinik bilgileri başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
