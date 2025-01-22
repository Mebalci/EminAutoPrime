using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EminAutoPrime.Data;
using EminAutoPrime.Models;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;

namespace EminAutoPrime.Controllers
{
    [Authorize(Roles = "ServisCalisani")]
    public class ServisController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServisController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var servisler = _context.ServisKayitlari
                .Include(s => s.Arac)
                .ThenInclude(a => a.Marka)
                .Include(s => s.Arac.Model)
                .Include(s => s.Durum)
                .Include(s => s.ServisIslemleri)
                .ThenInclude(si => si.ServisAlani)
                .Select(s => new ServisViewModel
                {
                    ServisId = s.ServisId,
                    AracBilgisi = $"{s.Arac.Plaka} - {s.Arac.Marka.MarkaAdi} {s.Arac.Model.ModelAdi}",
                    BaslangicTarihi = s.BaslangicTarihi,
                    TahminiBitisTarihi = s.TahminiBitisTarihi,
                    Durum = s.Durum.DurumAdi,
                    Aciklama = s.Aciklama,
                    ToplamMaliyet = s.ToplamMaliyet,
                    ServisAlani = s.ServisIslemleri
                    .Select(si => si.ServisAlani.ServisAdi)
                    .FirstOrDefault()
                }).ToList();

            return View(servisler);
        }
        public IActionResult Detay(int servisId)
        {
            var servis = _context.ServisKayitlari
                .Include(s => s.Arac)
                .ThenInclude(a => a.Marka)
                .Include(s => s.Arac.Model)
                .Include(s => s.Durum)
                .FirstOrDefault(s => s.ServisId == servisId);

            if (servis == null)
                return NotFound();

            var servisIslemleri = _context.ServisIslemleri
                .Include(si => si.ServisAlani)
                .Include(si => si.TakilanParca)
                .Include(si => si.IslemTip)
                .Where(si => si.AracId == servis.AracId)  
                .ToList();

            var servisIslemleri2 = _context.ServisIslemleri
                .Where(si => si.ServisId == servisId)
                .ToList();

            if (!servisIslemleri2.Any())
            {                
                var servisAlani = _context.ServisAlanlari.FirstOrDefault();
                if (servisAlani == null)
                {
                    TempData["ErrorMessage"] = "Servis alanı bulunamadı. Lütfen servis alanı ekleyin.";
                    return RedirectToAction("Index");
                }
                              
                var calisanId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(calisanId))
                {
                    TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                    return RedirectToAction("Index");
                }
                
                var yeniServisIslem = new ServisIslemleri
                {
                    ServisId = servisId,
                    CalisanId = calisanId,
                    ServisAlaniId = servisAlani.ServisAlaniId, 
                    IslemAciklama = $"Servis işlemi başlatıldı. Tahmini maliyet: {servis.ToplamMaliyet}",
                    IslemMaliyeti = 0,
                    IslemTarihi = DateTime.Now,
                    IslemTipId = 1, 
                    AracId = servis.AracId
                };

                _context.ServisIslemleri.Add(yeniServisIslem);
                _context.SaveChanges();
                                
                return RedirectToAction("ServisAlaniSec", new { servisId });
            }

            var islemler = servisIslemleri
                .GroupBy(si => si.ServisId)
                .Select(g => new IslemViewModel
                {
                    IslemId = g.Key,
                    IslemAdi = g.OrderByDescending(si => si.IslemTarihi).FirstOrDefault()?.ServisAlani?.ServisAdi ?? "Belirtilmemiş",
                    IslemAciklama = string.Join("\n", g.Select(si => si.IslemAciklama)),
                    IslemMaliyeti = g.Sum(si => si.IslemMaliyeti),
                    IslemTarihi = g.Max(si => si.IslemTarihi)
                })
                .ToList();

            var toplamMaliyet = servisIslemleri.Sum(si => si.IslemMaliyeti);

            var parcalar = servisIslemleri
                .Where(si => si.TakilanParca != null && si.ServisId == servisId)
                .Select(si => new ParcaViewModel
                {
                    ParcaAdi = si.TakilanParca.ParcaAdi,
                    Fiyat = si.TakilanParca.Fiyat
                })
                .ToList();

            var viewModel = new ServisKayitDetayViewModel
            {
                ServisId = servis.ServisId,
                AracBilgisi = $"{servis.Arac?.Plaka ?? "Plaka Yok"} - {servis.Arac?.Marka?.MarkaAdi ?? "Marka Yok"} {servis.Arac?.Model?.ModelAdi ?? "Model Yok"}",
                BaslangicTarihi = servis.BaslangicTarihi,
                TahminiBitisTarihi = servis.TahminiBitisTarihi,
                Durum = servis.Durum?.DurumAdi ?? "Durum Belirtilmemiş",
                Aciklama = servis.Aciklama ?? "",
                ToplamMaliyet = toplamMaliyet,
                Parcalar = parcalar,
                Islemler = islemler,
                ServisAlani = servisIslemleri
                    .Where(si => si.ServisId == servis.ServisId)  
                    .OrderByDescending(si => si.IslemTarihi)     
                    .Select(si => si.ServisAlani?.ServisAdi ?? "Servis Alanı Yok")
                    .FirstOrDefault(),
                ServisAlaniId = servisIslemleri
                    .Select(si => si.ServisAlani?.ServisAlaniId ?? 1)
                    .FirstOrDefault(),
            };

            return View("Detay", viewModel);
        }

        [HttpGet]
        public IActionResult ServisAlaniSec(int servisId)
        {
            var servisAlanlari = _context.ServisAlanlari
                .Select(sa => new SelectListItem
                {
                    Value = sa.ServisAlaniId.ToString(),
                    Text = sa.ServisAdi
                })
                .ToList();

            ViewBag.ServisAlanlari = servisAlanlari;
            ViewBag.ServisId = servisId;

            return View();
        }

        [HttpPost]
        public IActionResult ServisAlaniSec(int servisId, int servisAlaniId)
        {
            var servisIslem = _context.ServisIslemleri.FirstOrDefault(si => si.ServisId == servisId);
            if (servisIslem == null)
                return NotFound();


            servisIslem.ServisAlaniId = servisAlaniId;
            servisIslem.IslemTarihi = DateTime.Now;
            servisIslem.IslemAciklama = $"Servis alanı güncellendi: {_context.ServisAlanlari.Find(servisAlaniId)?.ServisAdi}";
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Servis alanı başarıyla güncellendi.";
            return RedirectToAction("Detay", new { servisId });
        }

        [HttpPost]
        public IActionResult ParcaEkle(int servisId, int parcaId, int servisAlaniId, string? ekAciklama)
        {
            var parca = _context.Parcalar.FirstOrDefault(p => p.ParcaId == parcaId);
            if (parca == null || parca.StokMiktari <= 0)
            {
                return Json(new { success = false, message = "Parça bulunamadı veya stokta yok." });
            }

            parca.StokMiktari -= 1;
            _context.Parcalar.Update(parca);
            _context.SaveChanges();

            var servisAlani = _context.ServisAlanlari.FirstOrDefault(sa => sa.ServisAlaniId == servisAlaniId);
            if (servisAlani == null)
            {
                return Json(new { success = false, message = "Seçilen servis alanı bulunamadı." });
            }

            var calisanId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(calisanId))
            {
                return Json(new { success = false, message = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın." });
            }

            var islemAciklama = $"Parça takıldı: {parca.ParcaAdi} - Parça fiyatı: {parca.Fiyat} - Servis Alanı: {servisAlani.ServisAdi}";
            if (!string.IsNullOrEmpty(ekAciklama))
            {
                islemAciklama += $" - Ek Açıklama: {ekAciklama}";
            }

            var aracId = _context.ServisKayitlari
                .Where(s => s.ServisId == servisId)
                .Select(s => s.AracId)
                .FirstOrDefault();

            if (aracId == 0)
            {
                return Json(new { success = false, message = "Araç bilgisi alınamadı. Lütfen tekrar deneyin." });
            }

            var yeniServisIslem = new ServisIslemleri
            {
                ServisId = servisId,
                TakilanParcaId = parca.ParcaId,
                IslemAciklama = islemAciklama,
                IslemMaliyeti = parca.Fiyat,
                IslemTarihi = DateTime.Now,
                CalisanId = calisanId,
                IslemTipId = 1,
                ServisAlaniId = servisAlaniId,
                AracId = aracId
            };

            _context.ServisIslemleri.Add(yeniServisIslem);

            var servisKaydi = _context.ServisKayitlari.FirstOrDefault(s => s.ServisId == servisId);
            if (servisKaydi != null)
            {
                servisKaydi.ToplamMaliyet += parca.Fiyat;
                _context.ServisKayitlari.Update(servisKaydi);
            }

            _context.SaveChanges();

            return Json(new { success = true, message = "Parça başarıyla eklendi." });
        }        

        [HttpGet]
        public IActionResult GetParcaKategori()
        {
            var kategoriler = _context.ParcaKategorileri
                .Select(k => new { k.KategoriId, k.KategoriAdi })
                .ToList();
            return Json(kategoriler);
        }

        [HttpGet]
        public IActionResult GetParcalarByKategori(int kategoriId)
        {
            var parcalar = _context.Parcalar
                .Where(p => p.KategoriId == kategoriId)
                .Select(p => new { p.ParcaId, p.ParcaAdi, p.Fiyat })
                .ToList();
            return Json(parcalar);
        }

        [HttpGet]
        public IActionResult GetServisDurumlari()
        {
            var servisDurumlari = _context.ServisDurumlari
                .Select(sd => new { sd.DurumId, sd.DurumAdi })
                .ToList();
            return Json(servisDurumlari);
        }

        [HttpPost]
        public IActionResult ServisGuncelle(int servisId, int durumId)
        {             
            var servis = _context.ServisKayitlari.FirstOrDefault(s => s.ServisId == servisId);
            if (servis == null)
            {
                return Json(new { success = false, message = "Servis bulunamadı." });
            }
            
            var durum = _context.ServisDurumlari.FirstOrDefault(d => d.DurumId == durumId);
            if (durum == null)
            {
                return Json(new { success = false, message = "Seçilen durum geçerli değil." });
            }

            servis.DurumId = durumId;
            
            _context.SaveChanges();

            return Json(new { success = true, message = "Servis durumu başarıyla güncellendi." });
        }
                
        [HttpPost]
        public IActionResult IslemEkle(
            int servisId,
            string? islemAciklama,
            decimal? islemMaliyeti,
            DateTime? tahminiBitisTarihi,
            int? servisAlaniId)
        {
            // Servis kaydını kontrol et
            var servis = _context.ServisKayitlari.FirstOrDefault(s => s.ServisId == servisId);
            if (servis == null)
            {
                return Json(new { success = false, message = "Servis bulunamadı." });
            }

            // İşlem açıklamasını kontrol et
            if (string.IsNullOrEmpty(islemAciklama))
            {
                return Json(new { success = false, message = "İşlem açıklaması zorunludur." });
            }

            // Servis alanını kontrol et (opsiyonel)
            if (servisAlaniId.HasValue)
            {
                var servisAlani = _context.ServisAlanlari.FirstOrDefault(sa => sa.ServisAlaniId == servisAlaniId);
                if (servisAlani == null)
                {
                    return Json(new { success = false, message = "Seçilen servis alanı geçerli değil." });
                }
            }

            // Kullanıcı kimliğini kontrol et
            var calisanId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(calisanId))
            {
                return Json(new { success = false, message = "Kullanıcı bilgisi alınamadı." });
            }

            var aracId = _context.ServisKayitlari
                .Where(s => s.ServisId == servisId)
                .Select(s => s.AracId)
                .FirstOrDefault();

            if (aracId == 0)
            {
                return Json(new { success = false, message = "Araç bilgisi alınamadı. Lütfen tekrar deneyin." });
            }

            // Yeni işlem kaydı oluştur
            var yeniIslem = new ServisIslemleri
            {
                ServisId = servisId,
                IslemAciklama = islemAciklama,
                IslemTarihi = DateTime.Now,
                CalisanId = calisanId,
                IslemTipId = 1,
                AracId = aracId
            };

            // Maliyet eklemesi
            if (islemMaliyeti.HasValue && islemMaliyeti > 0)
            {
                yeniIslem.IslemMaliyeti = islemMaliyeti.Value;
                servis.ToplamMaliyet += islemMaliyeti.Value;
            }

            // Tahmini bitiş tarihi güncellemesi
            if (tahminiBitisTarihi.HasValue)
            {
                servis.TahminiBitisTarihi = tahminiBitisTarihi.Value;
            }

            // Servis alanı güncellemesi
            if (servisAlaniId.HasValue)
            {
                yeniIslem.ServisAlaniId = servisAlaniId.Value;
            }

            // İşlemi ve güncellemeleri kaydet
            _context.ServisIslemleri.Add(yeniIslem);
            _context.ServisKayitlari.Update(servis);
            _context.SaveChanges();

            return Json(new { success = true, message = "İşlem başarıyla eklendi ve güncellemeler yapıldı." });
        }





    }
}
