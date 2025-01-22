using Microsoft.AspNetCore.Mvc;
using EminAutoPrime.Data;
using EminAutoPrime.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using ClosedXML.Excel;

namespace EminAutoPrime.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<AplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        private string FormatName(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var culture = new CultureInfo("tr-TR");
            text = text.ToLower(culture);
            return char.ToUpper(text[0], culture) + text.Substring(1);
        }

        public async Task<IActionResult> MusteriYonetimi(string searchTerm, int page = 1)
        {
            int pageSize = 5;
            var users = _userManager.Users.ToList();

            
            if (!string.IsNullOrEmpty(searchTerm))
            {
                users = users.Where(u =>
                   u.KullaniciAdi.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                   u.KullaniciSoyadi.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                   u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            int totalUsers = users.Count;
            int totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            var paginatedUsers = users.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var viewModel = new List<MusterilerViewModel>();
            foreach (var user in paginatedUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                viewModel.Add(new MusterilerViewModel
                {
                    UserId = user.Id,
                    Ad = user.KullaniciAdi,
                    Soyad=user.KullaniciSoyadi,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles.ToList()
                });
            }

            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;
            ViewData["SearchTerm"] = searchTerm;

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(int page = 1)
        {
            int pageSize = 5;
            var users = _userManager.Users.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var viewModel = new List<MusterilerViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                viewModel.Add(new MusterilerViewModel
                {
                    UserId = user.Id,
                    Ad = user.KullaniciAdi,
                    Soyad= user.KullaniciSoyadi,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles.ToList()
                });
            }

            return PartialView("_UserTablePartial", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(string firstname, string lastname, string email, string phoneNumber, string password)
        {
            if (string.IsNullOrWhiteSpace(firstname) || string.IsNullOrWhiteSpace(lastname) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest(new { message = "Tüm alanlar doldurulmalıdır." });
            }
                        
            firstname = FormatName(firstname);
            lastname = FormatName(lastname);

            var user = new AplicationUser
            {
                KullaniciAdi = firstname,
                KullaniciSoyadi = lastname,
                UserName = email,
                Email = email,
                PhoneNumber = phoneNumber
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Musteri");
                return Json(new { message = "Kullanıcı başarıyla oluşturuldu." });
            }
            else
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { message = string.Join(", ", errors) });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserDetails(string userId, string firstname, string lastname, string email, string phone, string role)
        {
            if (string.IsNullOrWhiteSpace(firstname))
            {
                return BadRequest("Kullanıcı Adı zorunludur.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }
            
            user.KullaniciAdi = !string.IsNullOrWhiteSpace(firstname) ? FormatName(firstname) : user.KullaniciAdi;
            user.KullaniciSoyadi = !string.IsNullOrWhiteSpace(lastname) ? FormatName(lastname) : user.KullaniciSoyadi;
            user.UserName = !string.IsNullOrWhiteSpace(email) ? email : user.Email;
            user.PhoneNumber = !string.IsNullOrWhiteSpace(phone) ? phone : user.PhoneNumber;
            user.Email = !string.IsNullOrWhiteSpace(email) ? email : user.Email;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return BadRequest("Kullanıcı bilgileri güncellenirken bir hata oluştu.");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeRoleResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!removeRoleResult.Succeeded)
            {
                return BadRequest("Kullanıcının mevcut rolleri kaldırılırken bir hata oluştu.");
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                var addRoleResult = await _userManager.AddToRoleAsync(user, role);
                if (!addRoleResult.Succeeded)
                {
                    return BadRequest("Kullanıcıya yeni rol atanırken bir hata oluştu.");
                }
            }

            return Ok("Kullanıcı bilgileri başarıyla güncellendi.");
        }


        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("Kullanıcı ID'si zorunludur.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                return BadRequest("Kullanıcı silinirken bir hata oluştu.");
            }

            return Ok("Kullanıcı başarıyla silindi.");
        }


        [HttpGet]
        public IActionResult GetUserVehicles(string userId)
        {
            var vehicles = _context.Araclar
                .Where(v => v.SahipId == userId)
                .Select(v => new
                {
                    v.Plaka,
                    MarkaAdi = v.Marka.MarkaAdi,
                    ModelAdi = v.Model.ModelAdi,
                    v.Yil
                })
                .ToList();

            return Json(vehicles);
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            var userRoles = _userManager.GetRolesAsync(user).Result;

            var model = new MusterilerViewModel
            {
                UserId = user.Id,
                Ad = user.KullaniciAdi,
                Soyad= user.KullaniciSoyadi,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = roles.ToList(),
                SelectedRole = userRoles.FirstOrDefault()
            };

            return View(model); 
        }       

        public IActionResult Index()
        {
            var viewModel = new EminAutoAdminViewModel
            {                
                ToplamKampanyalar = _context.Kampanyalar.Count(k => k.BitisTarihi >= DateTime.Now),
                Kampanyalar = _context.Kampanyalar.Where(k => k.BitisTarihi >= DateTime.Now).ToList()
            };

            return View(viewModel);
        }


        public async Task<IActionResult> AktifKampanyalar()
        {
            var kampanyalar = await _context.Kampanyalar
                .OrderBy(k => k.BaslangicTarihi)
                .ToListAsync();

            return View(kampanyalar); 
        }

        public async Task<IActionResult> Araclar()
        {            
            return View();
        }

        [HttpGet]
        public IActionResult GetMarkalar(int page = 1, string search = "")
        {
            var markalar = _context.AracMarkalari
                .Where(m => string.IsNullOrEmpty(search) || m.MarkaAdi.Contains(search))
                .OrderBy(m => m.MarkaAdi)
                .Skip((page - 1) * 5)
                .Take(5)
                .Select(m => new { id = m.MarkaId, markaAdi = m.MarkaAdi })
                .ToList();

            return Json(new { data = markalar });
        }

        [HttpGet]
        public IActionResult GetMarkalarEkleme()
        {
            var markalar = _context.AracMarkalari
                .Select(m => new { id = m.MarkaId, markaAdi = m.MarkaAdi })
                .ToList();

            return Json(markalar);
        }

        [HttpGet]
        public IActionResult GetModeller(int page = 1, string search = "")
        {
            var modeller = _context.AracModelleri
                .Include(m => m.Marka)
                .Where(m => string.IsNullOrEmpty(search) || m.ModelAdi.Contains(search))
                .OrderBy(m => m.ModelAdi)
                .Skip((page - 1) * 5)
                .Take(5)
                .Select(m => new
                {
                    id = m.ModelId,
                    modelAdi = m.ModelAdi,
                    markaAdi = m.Marka.MarkaAdi
                })
                .ToList();

            var totalModels = _context.AracModelleri
                .Count(m => string.IsNullOrEmpty(search) || m.ModelAdi.Contains(search));

            var totalPages = (int)Math.Ceiling((double)totalModels / 5);

            return Json(new { data = modeller, totalPages });
        }

        [HttpPost]
        public IActionResult AddMarka(string markaAdi)
        {
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            string formattedModelAdi = textInfo.ToTitleCase(markaAdi.ToLower());

            _context.AracMarkalari.Add(new AracMarkalari { MarkaAdi = formattedModelAdi });
            _context.SaveChanges();
            return Ok();
        }

        [HttpPost]
        public IActionResult AddModel(int markaId, string modelAdi)
        {
            if (!string.IsNullOrWhiteSpace(modelAdi))
            {               
                TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
                string formattedModelAdi = textInfo.ToTitleCase(modelAdi.ToLower());

                _context.AracModelleri.Add(new AracModelleri { MarkaId = markaId, ModelAdi = formattedModelAdi });
                _context.SaveChanges();
                return Ok();
            }
            return BadRequest("Model adı boş olamaz.");
        }

        [HttpPost]
        public IActionResult DeleteMarka(int id)
        {
            var marka = _context.AracMarkalari.Find(id);
            if (marka != null)
            {
                _context.AracMarkalari.Remove(marka);
                _context.SaveChanges();
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult DeleteModel(int id)
        {
            var model = _context.AracModelleri.Find(id);
            if (model != null)
            {
                _context.AracModelleri.Remove(model);
                _context.SaveChanges();
                return Ok();
            }
            return NotFound("Model bulunamadı.");
        }

        ///////////// 
        ///

        [HttpGet]
        public IActionResult ParcaYönetimi()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetCategories()
        {
            var categories = _context.ParcaKategorileri
                .Select(k => new
                {
                    id = k.KategoriId,
                    name = k.KategoriAdi
                })
                .ToList();

            return Json(categories);
        }

        [HttpGet]
        public IActionResult GetParts(int page = 1)
        {
            int pageSize = 5;
            var totalRecords = _context.Parcalar.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var parts = _context.Parcalar
                .Include(p => p.Kategori)
                .OrderBy(p => p.ParcaId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    id = p.ParcaId,
                    partName = p.ParcaAdi,
                    price = p.Fiyat,
                    stock = p.StokMiktari,
                    categoryName = p.Kategori.KategoriAdi
                })
                .ToList();

            return Json(new { data = parts, totalPages });
        }

        [HttpPost]
        public IActionResult AddPart(string partName, decimal price, int stock, int categoryId)
        {
            if (string.IsNullOrWhiteSpace(partName) || price <= 0 || stock < 0 || !_context.ParcaKategorileri.Any(k => k.KategoriId == categoryId))
            {
                return BadRequest("Geçerli bir değer girin.");
            }

            partName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(partName.Trim().ToLower());

            if (_context.Parcalar.Any(p => p.ParcaAdi == partName && p.KategoriId == categoryId))
            {
                return BadRequest("Bu parça zaten mevcut.");
            }

            _context.Parcalar.Add(new Parcalar
            {
                ParcaAdi = partName,
                Fiyat = price,
                StokMiktari = stock,
                KategoriId = categoryId
            });
            _context.SaveChanges();
            return Ok();
        }

        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            var fileName = "ParcalarSablonu.xlsx";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", fileName);
            return PhysicalFile(filePath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet]
        public IActionResult DownloadCategories()
        {
            var fileName = "Kategoriler.xlsx";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", fileName);
                        
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Kategoriler");
                                
                worksheet.Cell(1, 1).Value = "Kategori ID";
                worksheet.Cell(1, 2).Value = "Kategori Adı";
                                
                var headerRange = worksheet.Range(1, 1, 1, 2);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                               
                var categories = _context.ParcaKategorileri.ToList();
                for (int i = 0; i < categories.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = categories[i].KategoriId;
                    worksheet.Cell(i + 2, 2).Value = categories[i].KategoriAdi;
                }

                worksheet.Columns().AdjustToContents(); 
                                
                workbook.SaveAs(filePath);
            }

            return PhysicalFile(filePath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        public IActionResult UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Lütfen geçerli bir dosya yükleyin.");
            }

            var errorMessages = new List<string>();
            var updateMessages = new List<string>();

            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);

                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

                    foreach (var row in rows)
                    {
                        try
                        {                            
                            if (row.CellsUsed().All(c => string.IsNullOrWhiteSpace(c.GetValue<string>())))
                            {
                                continue; 
                            }
                                                        
                            var partName = row.Cell(1).GetValue<string>().Trim();
                            if (string.IsNullOrWhiteSpace(partName))
                            {
                                throw new Exception("Parça Adı boş olamaz.");
                            }

                            var price = row.Cell(2).TryGetValue<decimal>(out decimal parsedPrice)
                                ? parsedPrice
                                : throw new Exception("Fiyat sayısal bir değer olmalıdır.");

                            var stock = row.Cell(3).TryGetValue<int>(out int parsedStock)
                                ? parsedStock
                                : throw new Exception("Stok sayısal bir değer olmalıdır.");

                            var categoryId = row.Cell(4).TryGetValue<int>(out int parsedCategoryId)
                                ? parsedCategoryId
                                : throw new Exception("Kategori ID sayısal bir değer olmalıdır.");
                                                       
                            if (!_context.ParcaKategorileri.Any(k => k.KategoriId == categoryId))
                            {
                                throw new Exception($"Kategori ID {categoryId} geçerli değil.");
                            }
                            
                            var existingPart = _context.Parcalar.FirstOrDefault(p => p.ParcaAdi == partName && p.KategoriId == categoryId);
                            if (existingPart != null)
                            {                              
                                var originalPrice = existingPart.Fiyat;
                                var originalStock = existingPart.StokMiktari;

                                if (existingPart.Fiyat != price || existingPart.StokMiktari != stock)
                                {
                                    existingPart.Fiyat = price;
                                    existingPart.StokMiktari = stock;
                                    updateMessages.Add($"'{partName}' parçası güncellendi. Eski Fiyat: {originalPrice}, Yeni Fiyat: {price}, Eski Stok: {originalStock}, Yeni Stok: {stock}");
                                }
                                else
                                {
                                    updateMessages.Add($"'{partName}' parçası zaten mevcut ve güncellenmedi.");
                                }

                                continue;
                            }

                           
                            _context.Parcalar.Add(new Parcalar
                            {
                                ParcaAdi = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(partName.ToLower()),
                                Fiyat = price,
                                StokMiktari = stock,
                                KategoriId = categoryId
                            });
                        }
                        catch (Exception ex)
                        {
                            errorMessages.Add($"Satır {row.RowNumber()} hata: {ex.Message}");
                        }
                    }

                    _context.SaveChanges();
                }
            }

            
            if (errorMessages.Any())
            {
                return BadRequest(string.Join("\n", errorMessages));
            }

            if (updateMessages.Any())
            {
                return Ok(string.Join("\n", updateMessages));
            }

            return Ok("Excel dosyası başarıyla yüklendi ve veriler kaydedildi.");
        }
        public IActionResult RandevuTalepleri()
        {
            var talepler = _context.Randevular
                                   .Include(r => r.ServisAlani)
                                   .Include(r => r.Arac)
                                   .Include(r => r.Durum)
                                   .ToList();

            return View(talepler);
        }

        [HttpPost]
        public IActionResult RandevuDurumuGuncelle(int randevuId, int durumId)
        {
            var randevu = _context.Randevular.Find(randevuId);
            if (randevu != null)
            {
                randevu.DurumId = durumId;
                _context.SaveChanges();
            }

            return RedirectToAction("RandevuTalepleri");
        }

    }


}

