using EminAutoPrime.Data;
using EminAutoPrime.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EminAutoPrime.Controllers
{
    [Authorize(Roles = "ServisCalisani, Admin")]
    public class ServisKayitController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AplicationUser> _userManager;

        public ServisKayitController(ApplicationDbContext context, UserManager<AplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        private string FormatName(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var culture = new CultureInfo("tr-TR");
            text = text.ToLower(culture);
            return char.ToUpper(text[0], culture) + text.Substring(1);
        }

        [HttpGet]
        public IActionResult Index()
        {            
            return View();
        }      
        
        [HttpGet]
        public IActionResult GetUsers(string searchTerm, int page = 1, int pageSize = 25)
        {
            try
            {
                var usersQuery = _userManager.Users.AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    usersQuery = usersQuery.Where(u =>
                        u.Email.Contains(searchTerm) ||
                        u.PhoneNumber.Contains(searchTerm) ||
                        u.KullaniciAdi.Contains(searchTerm));
                }

                var totalUsers = usersQuery.Count();
                var users = usersQuery
                    .OrderBy(u => u.KullaniciAdi)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var araclar = _context.Araclar
                    .Include(a => a.Marka)
                    .Include(a => a.Model)
                    .ToList();

                var servisDurumlari = _context.ServisDurumlari.ToList();
                var servisKayitlari = _context.ServisKayitlari.ToList();

                var userList = users.Select(u => new
                {
                    id = u.Id,
                    fullName = (u.KullaniciAdi ?? "") + " " + (u.KullaniciSoyadi ?? ""),
                    email = u.Email ?? "Bilinmiyor",
                    phone = u.PhoneNumber ?? "Bilinmiyor",
                    firstVehicle = araclar
                        .Where(a => a.SahipId == u.Id)
                        .Select(a => new
                        {
                            plaka = a.Plaka ?? "Plaka Yok",
                            markaAdi = a.Marka?.MarkaAdi ?? "Marka Yok",
                            modelAdi = a.Model?.ModelAdi ?? "Model Yok",
                            durum = servisDurumlari
                                .Where(d => servisKayitlari
                                    .Where(s => s.AracId == a.AracId)
                                    .OrderByDescending(s => s.BaslangicTarihi)
                                    .Select(s => s.DurumId)
                                    .FirstOrDefault() == d.DurumId)
                                .Select(d => d.DurumAdi)
                                .FirstOrDefault() ?? "Durum Yok"
                        })
                        .FirstOrDefault(),
                    otherVehicles = araclar
                        .Where(a => a.SahipId == u.Id)
                        .Skip(1) 
                        .Select(a => new
                        {
                            id = a.AracId,
                            plaka = a.Plaka,
                            markaAdi = a.Marka?.MarkaAdi ?? "Marka Yok",
                            modelAdi = a.Model?.ModelAdi ?? "Model Yok"
                        })
                        .ToList(),
                    status = araclar
                        .Where(a => a.SahipId == u.Id)
                        .Select(a =>
                        {
                            var servisKaydi = servisKayitlari
                                .Where(s => s.AracId == a.AracId)
                                .OrderByDescending(s => s.BaslangicTarihi)
                                .FirstOrDefault();

                            var durumAdi = servisDurumlari
                                .Where(d => d.DurumId == servisKaydi?.DurumId)
                                .Select(d => d.DurumAdi)
                                .FirstOrDefault();

                            return durumAdi ?? "Durum Yok";
                        })
                        .FirstOrDefault() ?? "Durum Yok"
                }).ToList();

                var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

                return Json(new { users = userList, currentPage = page, totalPages });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateUser(string email, string firstName, string lastName, string phoneNumber)
        {
            try
            {             
                var user = new AplicationUser
                {
                    KullaniciAdi = (FormatName(firstName) ?? "Emin Auto Ad"),
                    KullaniciSoyadi = (FormatName(lastName) ?? "Emin Auto Soyad"),
                    UserName = email,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(user, "Default123!");
                if (result.Succeeded)
                {                    
                    var roleResult = await _userManager.AddToRoleAsync(user, "Musteri");
                    if (!roleResult.Succeeded)
                    {
                        return Json(new { success = false, message = "Kullanıcı oluşturuldu ancak rol atanamadı." });
                    }
                    return Json(new { success = true, user });
                }
                return Json(new { success = false, message = "Kullanıcı oluşturulamadı." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }
                
        [HttpPost]
        public IActionResult AddArac(string plaka, int markaId, int modelId, string sahipId)
        {
            try
            {                
                if (_context.Araclar.Any(a => a.Plaka == plaka))
                {
                    return Json(new { success = false, message = "Bu plaka zaten kayıtlı." });
                }

                var arac = new Araclar
                {
                    Plaka = plaka,
                    MarkaId = markaId,
                    ModelId = modelId,
                    SahipId = sahipId
                };

                _context.Araclar.Add(arac);
                _context.SaveChanges();

                return Json(new { success = true, message = "Araç başarıyla eklendi.", arac });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }
                
        [HttpPost]
        public IActionResult AddServisKayit(int aracId, int durumId, string aciklama)
        {
            try
            {
                var servis = new ServisKayitlari
                {
                    AracId = aracId,
                    BaslangicTarihi = DateTime.Now,
                    DurumId = durumId,
                    Aciklama = aciklama
                };
                _context.ServisKayitlari.Add(servis);
                _context.SaveChanges();
                return Json(new { success = true, servis });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMarkalar()
        {
            try
            {
                var markalar = await _context.AracMarkalari.ToListAsync();
                return Json(markalar);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetModeller(int markaId)
        {
            try
            {
                var modeller = await _context.AracModelleri.Where(m => m.MarkaId == markaId).ToListAsync();
                return Json(modeller);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult CreateServisKayit(string kullaniciId)
        {
            var araclar = _context.Araclar
                 .Include(a => a.Marka)
                 .Include(a => a.Model)
                 .Include(a => a.Sahip) 
                 .Where(a => a.SahipId == kullaniciId)
                 .ToList();

            if (!araclar.Any())
            {
                var errorModel = new ErrorViewModel
                {
                    ErrorMessage = "Bu kullanıcıya ait bir araç bulunamadı.",
                    RequestId = HttpContext.TraceIdentifier
                };
                return View("Error", errorModel);
            }

            var viewModel = new ServisKayitViewModel
            {
                KullaniciAdi = araclar.First().Sahip != null
                    ? $"{araclar.First().Sahip.KullaniciAdi} {araclar.First().Sahip.KullaniciSoyadi}"
                    : "Sahip Bilgisi Eksik",
                AracListesi = araclar.Select(a => new SelectListItem
                {
                    Value = a.AracId.ToString(),
                    Text = $"{a.Plaka} - {a.Marka?.MarkaAdi} {a.Model?.ModelAdi}"
                }).ToList(),
                DurumListesi = _context.ServisDurumlari
                    .Select(d => new SelectListItem
                    {
                        Value = d.DurumId.ToString(),
                        Text = d.DurumAdi
                    }).ToList()
            };

            return View(viewModel);

        }


        [HttpPost]
        public IActionResult CreateServisKayit(
            int AracId, 
            DateTime BaslangicTarihi, 
            DateTime TahminiBitisTarihi, 
            int DurumId,
            string Aciklama, 
            int ToplamMaliyet)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                                        .SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
                return Json(new { success = false, message = "Form verileri geçersiz.", errors });
            }

            try
            {
                var servisKaydi = new ServisKayitlari
                {
                    AracId = AracId,
                    BaslangicTarihi = BaslangicTarihi,
                    TahminiBitisTarihi = TahminiBitisTarihi,
                    DurumId = DurumId,
                    Aciklama = Aciklama,
                    ToplamMaliyet = ToplamMaliyet
                };

                _context.ServisKayitlari.Add(servisKaydi);
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult EditServisKayit(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index");
            }

            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            var vehicles = _context.Araclar
                .Where(a => a.SahipId == userId)
                .Select(a => new SelectListItem
                {
                    Value = a.AracId.ToString(),
                    Text = $"{a.Plaka} - {a.Marka.MarkaAdi ?? "Marka Yok"} {a.Model.ModelAdi ?? "Model Yok"}"
                }).ToList();

            var markaListesi = _context.AracMarkalari
                .Select(m => new SelectListItem
                {
                    Value = m.MarkaId.ToString(),
                    Text = m.MarkaAdi
                }).ToList();

            var model = new ServisKayitDüzenleViewModel
            {
                KullaniciId = user.Id,
                KullaniciAdi = user.KullaniciAdi,
                KullaniciSoyadi = user.KullaniciSoyadi,
                Email = user.Email,
                Telefon = user.PhoneNumber,
                Araçlar = vehicles,
                MarkaListesi = markaListesi
            };

            return View(model);
        }
                
        [HttpPost]
        public async Task<IActionResult> UpdateUser(string kullaniciId, string kullaniciAdi, string kullaniciSoyadi, string telefon, string email)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(kullaniciId);
                if (user == null)
                {
                    return Json(new { success = false, message = "Kullanıcı bulunamadı." });
                }

                user.KullaniciAdi = kullaniciAdi;
                user.KullaniciSoyadi = kullaniciSoyadi;
                user.PhoneNumber = telefon;
                user.Email = email;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Kullanıcı bilgileri güncellenemedi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }
       
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string kullaniciId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(kullaniciId);
                if (user == null)
                {
                    return Json(new { success = false, message = "Kullanıcı bulunamadı." });
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Kullanıcı silinemedi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }
               
        [HttpPost]
        [Authorize(Roles = "Musteri, ServisCalisani, Admin")]
        public IActionResult AddVehicle(string plaka, int markaId, int modelId, string sahipId)
        {
            try
            {
                if (_context.Araclar.Any(a => a.Plaka == plaka))
                {
                    return Json(new { success = false, message = "Bu plaka zaten kayıtlı." });
                }

                var arac = new Araclar
                {
                    Plaka = plaka,
                    MarkaId = markaId,
                    ModelId = modelId,
                    SahipId = sahipId
                };

                _context.Araclar.Add(arac);
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }
               
        [HttpPost]
        public IActionResult DeleteVehicle(int araçId)
        {
            try
            {
                var araç = _context.Araclar.FirstOrDefault(a => a.AracId == araçId);
                if (araç == null)
                {
                    return Json(new { success = false, message = "Araç bulunamadı." });
                }

                _context.Araclar.Remove(araç);
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult GetVehicleById(int vehicleId)
        {
            try
            {
                var vehicle = _context.Araclar
                    .Include(a => a.Marka)
                    .Include(a => a.Model)
                    .FirstOrDefault(a => a.AracId == vehicleId);

                if (vehicle == null)
                    return Json(new { success = false, message = "Araç bulunamadı." });

                return Json(new
                {
                    success = true,
                    vehicle = new
                    {
                        vehicle.Plaka,
                        MarkaId = vehicle.MarkaId,
                        ModelId = vehicle.ModelId
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult UpdateVehicle(int vehicleId, string plaka, int markaId, int modelId)
        {
            try
            {
                var vehicle = _context.Araclar.FirstOrDefault(a => a.AracId == vehicleId);
                if (vehicle == null)
                    return Json(new { success = false, message = "Araç bulunamadı." });

                vehicle.Plaka = plaka;
                vehicle.MarkaId = markaId;
                vehicle.ModelId = modelId;

                _context.Araclar.Update(vehicle);
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }



    }
}
