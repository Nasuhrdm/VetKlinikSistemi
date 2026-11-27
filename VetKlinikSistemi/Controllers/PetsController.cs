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
    public class PetsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public PetsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var pets = await _context.Pets
                .Include(p => p.VaccinationRecords)
                .Include(p => p.Appointments)
                .Where(p => p.OwnerId == userId)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(pets);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            var pet = await _context.Pets
                .Include(p => p.VaccinationRecords)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.ClinicService)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Veterinarian)
                .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);

            if (pet == null)
                return NotFound();

            ViewBag.NewVaccination = new VaccinationCreateViewModel
            {
                PetId = pet.Id,
                DateAdministered = DateTime.Today
            };

            return View(pet);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new PetFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PetFormViewModel model)
        {
            if (model.BirthDate > DateTime.Today)
            {
                ModelState.AddModelError("BirthDate", "Doğum tarihi bugünden ileri bir tarih olamaz.");
            }

            string? photoUrl = null;
            if (model.PhotoFile != null && model.PhotoFile.Length > 0)
            {
                var uploadResult = await SavePetPhotoAsync(model.PhotoFile);
                if (!uploadResult.Success)
                {
                    ModelState.AddModelError("PhotoFile", uploadResult.ErrorMessage!);
                }
                else
                {
                    photoUrl = uploadResult.FileUrl;
                }
            }

            if (!ModelState.IsValid)
                return View(model);

            var userId = _userManager.GetUserId(User)!;
            var pet = new Pet
            {
                Name = model.Name,
                Species = model.Species,
                Breed = model.Breed,
                BirthDate = model.BirthDate,
                Weight = model.Weight,
                PhotoUrl = photoUrl,
                OwnerId = userId
            };

            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{pet.Name} başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);

            if (pet == null)
                return NotFound();

            var model = new PetFormViewModel
            {
                Id = pet.Id,
                Name = pet.Name,
                Species = pet.Species,
                Breed = pet.Breed,
                BirthDate = pet.BirthDate,
                Weight = pet.Weight,
                ExistingPhotoUrl = pet.PhotoUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PetFormViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == model.Id && p.OwnerId == userId);

            if (pet == null)
                return NotFound();

            if (model.BirthDate > DateTime.Today)
            {
                ModelState.AddModelError("BirthDate", "Doğum tarihi bugünden ileri bir tarih olamaz.");
            }

            if (model.PhotoFile != null && model.PhotoFile.Length > 0)
            {
                var uploadResult = await SavePetPhotoAsync(model.PhotoFile);
                if (!uploadResult.Success)
                {
                    ModelState.AddModelError("PhotoFile", uploadResult.ErrorMessage!);
                }
                else
                {
                    DeleteOldPhoto(pet.PhotoUrl);
                    pet.PhotoUrl = uploadResult.FileUrl;
                }
            }

            if (!ModelState.IsValid)
            {
                model.ExistingPhotoUrl = pet.PhotoUrl;
                return View(model);
            }

            pet.Name = model.Name;
            pet.Species = model.Species;
            pet.Breed = model.Breed;
            pet.BirthDate = model.BirthDate;
            pet.Weight = model.Weight;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"{pet.Name} bilgileri güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var pet = await _context.Pets
                .Include(p => p.VaccinationRecords)
                .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);

            if (pet == null)
                return NotFound();

            bool hasAppointments = await _context.Appointments.AnyAsync(a => a.PetId == id);
            if (hasAppointments)
            {
                TempData["Error"] = "Bu evcil hayvana ait randevu kayıtları bulunduğu için silinemez.";
                return RedirectToAction(nameof(Index));
            }

            DeleteOldPhoto(pet.PhotoUrl);
            _context.VaccinationRecords.RemoveRange(pet.VaccinationRecords);
            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Evcil hayvan kaydı silindi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVaccination(VaccinationCreateViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == model.PetId && p.OwnerId == userId);

            if (pet == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Lütfen aşı adı ve tarih bilgilerini eksiksiz giriniz.";
                return RedirectToAction(nameof(Details), new { id = model.PetId });
            }

            if (model.NextDueDate.HasValue && model.NextDueDate.Value < model.DateAdministered)
            {
                TempData["Error"] = "Sonraki aşı tarihi uygulama tarihinden önce olamaz.";
                return RedirectToAction(nameof(Details), new { id = model.PetId });
            }

            _context.VaccinationRecords.Add(new VaccinationRecord
            {
                PetId = model.PetId,
                VaccineName = model.VaccineName,
                DateAdministered = model.DateAdministered,
                NextDueDate = model.NextDueDate,
                Notes = model.Notes
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Aşı kaydı eklendi.";
            return RedirectToAction(nameof(Details), new { id = model.PetId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVaccination(int id)
        {
            var userId = _userManager.GetUserId(User);
            var record = await _context.VaccinationRecords
                .Include(v => v.Pet)
                .FirstOrDefaultAsync(v => v.Id == id && v.Pet != null && v.Pet.OwnerId == userId);

            if (record == null)
                return NotFound();

            int petId = record.PetId;
            _context.VaccinationRecords.Remove(record);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Aşı kaydı silindi.";
            return RedirectToAction(nameof(Details), new { id = petId });
        }

        private async Task<(bool Success, string? FileUrl, string? ErrorMessage)> SavePetPhotoAsync(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(ext))
                return (false, null, "Sadece .jpg, .jpeg, .png veya .webp formatında fotoğraf yükleyebilirsiniz.");

            if (file.Length > 5 * 1024 * 1024)
                return (false, null, "Fotoğraf boyutu en fazla 5 MB olabilir.");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "pets");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return (true, $"/uploads/pets/{uniqueFileName}", null);
        }

        private void DeleteOldPhoto(string? photoUrl)
        {
            if (string.IsNullOrEmpty(photoUrl))
                return;

            var relativePath = photoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_env.WebRootPath, relativePath);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}
