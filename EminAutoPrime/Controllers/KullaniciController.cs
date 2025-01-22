using Microsoft.AspNetCore.Mvc;
using EminAutoPrime.Data;
using EminAutoPrime.Models;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EminAutoPrime.Controllers
{
    [Authorize(Roles = "Musteri, Admin")]
    public class KullaniciController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KullaniciController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult GöstergePaneli()
        {
            ViewData["ActivePage"] = "GöstergePaneli";

            var kampanyalar = _context.Kampanyalar
                .OrderByDescending(k => k.BaslangicTarihi)
                .Take(3)
                .ToList();

            var viewModel = new RandevuAlViewModel
            {
                Kampanyalar = kampanyalar
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_GöstergePaneli", viewModel);
            }

            return View(viewModel);
        }
        public IActionResult Araclar()
        {
            ViewData["CurrentUserId"] = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewData["ActivePage"] = "Araclar";
            var kullaniciId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var araclar = _context.Araclar
                .Where(a => a.SahipId == kullaniciId)
                .Select(a => new KullaniciAracViewModel
                {
                    AracId = a.AracId,
                    Plaka = a.Plaka,
                    Yil = a.Yil,
                    MarkaAdi = _context.AracMarkalari.FirstOrDefault(m => m.MarkaId == a.MarkaId).MarkaAdi,
                    ModelAdi = _context.AracModelleri.FirstOrDefault(m => m.ModelId == a.ModelId).ModelAdi,
                    ServisDurumu = _context.ServisKayitlari
                        .Where(s => s.AracId == a.AracId)
                        .OrderByDescending(s => s.BaslangicTarihi)
                        .Select(s => _context.ServisDurumlari.FirstOrDefault(d => d.DurumId == s.DurumId).DurumAdi)
                        .FirstOrDefault(),
                    ServisAciklama = _context.ServisKayitlari
                        .Where(s => s.AracId == a.AracId)
                        .OrderByDescending(s => s.BaslangicTarihi)
                        .Select(s => s.Aciklama)
                        .FirstOrDefault(),
                    ServisIslemleri = _context.ServisIslemleri
                        .Where(si => si.AracId == a.AracId)
                        .Select(si => new ServisDetayViewModel
                        {
                            ServisId = si.ServisId,
                            IslemId = si.IslemId,
                            CalisanAdi = _context.Users.FirstOrDefault(u => u.Id == si.CalisanId).UserName,
                            ServisAlanAdi = _context.ServisAlanlari.FirstOrDefault(sa => sa.ServisAlaniId == si.ServisId).ServisAdi,
                            TakilanParcaAdi = _context.Parcalar.FirstOrDefault(p => p.ParcaId == si.TakilanParcaId).ParcaAdi,
                            IslemTipi = _context.ServisIslemTipleri.FirstOrDefault(it => it.IslemTipId == si.IslemTipId).IslemAdi,
                            IslemAciklama = si.IslemAciklama,
                            IslemMaliyeti = si.IslemMaliyeti,
                            IslemTarihi = si.IslemTarihi
                        })
                        .ToList()
                })
                .ToList();

            return PartialView("_Araclar", araclar);
        }

        public IActionResult AracEkle()
        {
            var markaListesi = _context.AracMarkalari
                .Select(m => new SelectListItem
                {
                    Value = m.MarkaId.ToString(),
                    Text = m.MarkaAdi
                })
                .ToList();

            var viewModel = new AddVehicleViewModel
            {
                MarkaListesi = markaListesi
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult AracEkle(AddVehicleViewModel model)
        {        

            if (_context.Araclar.Any(a => a.Plaka == model.Plaka))
            {
                return Json(new { success = false, message = "Bu plaka zaten kayıtlı." });
            }

            var yeniArac = new Araclar
            {
                Plaka = model.Plaka,
                MarkaId = model.MarkaId,
                ModelId = model.ModelId,
                SahipId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Yil = model.Yil
            };

            _context.Araclar.Add(yeniArac);
            _context.SaveChanges();

            return Json(new { success = true });
        }



        [HttpGet]
        public IActionResult ModelGetir(int markaId)
        {
            var modeller = _context.AracModelleri
                .Where(m => m.MarkaId == markaId)
                .Select(m => new SelectListItem
                {
                    Value = m.ModelId.ToString(),
                    Text = m.ModelAdi
                })
                .ToList();

            return Json(modeller);
        }


        [HttpGet]
        public IActionResult SilmeOnayla(int aracId)
        {
            ViewData["AracId"] = aracId;
            return View();
        }

        [HttpPost]
        public IActionResult AraciSil(int aracId)
        {
            var arac = _context.Araclar.FirstOrDefault(a => a.AracId == aracId);
            if (arac == null)
            {
                return Json(new { success = false, message = "Araç bulunamadı." });
            }
                        
            var servisKayitVar = _context.ServisKayitlari.Any(sk => sk.AracId == aracId);
            var servisIslemVar = _context.ServisIslemleri.Any(si => si.AracId == aracId);

            if (servisKayitVar || servisIslemVar)
            {
                return Json(new { success = false, message = "Bu araç üzerinde aktif bir servis kaydı veya işlemi olduğu için silinemez." });
            }

            _context.Araclar.Remove(arac);
            _context.SaveChanges();

            return Json(new { success = true, message = "Araç başarıyla silindi.", redirectUrl = Url.Action("GöstergePaneli", "Kullanici") });
        }
        public IActionResult Randevular()
        {
            var kullaniciId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var randevular = _context.Randevular
                .Include(r => r.Durum)
                .Include(r => r.Arac)  
                .ThenInclude(a => a.Marka)  
                .Include(r => r.Arac.Model)  
                .Where(r => r.MusteriId == kullaniciId)
                .Select(r => new RandevuViewModel
                {
                    RandevuTarihi = r.RandevuTarihi,
                    TalepTarihi = r.TalepTarihi,
                    Aciklama = r.Aciklama,
                    Durum = r.Durum.DurumAdi,
                    AracBilgisi = $"{r.Arac.Plaka} - {r.Arac.Marka.MarkaAdi} {r.Arac.Model.ModelAdi}"  
                })
                .ToList();

            var viewModel = new RandevuTabViewModel
            {
                YaklasanRandevular = randevular.Where(r => r.RandevuTarihi > DateTime.Now && (r.Durum == "Beklemede" || r.Durum == "Onaylandı")).ToList(),
                GecmisRandevular = randevular.Where(r => r.RandevuTarihi <= DateTime.Now).ToList(),
                TamamlananRandevular = randevular.Where(r => r.Durum == "Tamamlandı").ToList()
            };

            return PartialView("_Randevular", viewModel);
        }


        [HttpGet]
        public IActionResult RandevuAl()
        {
            var kullaniciId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var araclar = _context.Araclar
                .Where(a => a.SahipId == kullaniciId)
                .Select(a => new SelectListItem
                {
                    Value = a.AracId.ToString(),
                    Text = $"{a.Plaka} - {a.MarkaId} {a.ModelId}"  
                })
                .ToList();

            ViewBag.Araclar = araclar;

            var servisAlanlari = _context.ServisAlanlari
                .Select(sa => new SelectListItem
                {
                    Value = sa.ServisAlaniId.ToString(),
                    Text = sa.ServisAdi  
                })
                .ToList();

            var viewModel = new RandevuAlViewModel
            {
                ServisAlanlari = servisAlanlari
            };

            return View(viewModel);
        }


        [HttpPost]
        public IActionResult RandevuAl(RandevuAlViewModel model)
        {            

            var yeniRandevu = new Randevular
            {
                MusteriId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                AracId = model.AracId,
                ServisAlaniId = model.SeciliServisAlaniId.Value,
                RandevuTarihi = model.RandevuTarihi.Value,
                TalepTarihi = DateTime.Now,
                Aciklama = model.Aciklama,
                DurumId = 1
            };

            _context.Randevular.Add(yeniRandevu);
            _context.SaveChanges();

            return Json(new { success = true, message = "Randevu başarıyla oluşturuldu.", 
                redirectUrl = Url.Action("GöstergePaneli", "Kullanici") 
            });
        }
        public IActionResult Yorumlar()
        {
            var yorumlar = _context.KullaniciYorumlari
                .Include(y => y.Kullanici)
                .OrderByDescending(y => y.OlusturulmaTarihi)
                .ToList();

            return PartialView("_Yorumlar",yorumlar);
        }

        [HttpGet]
        public IActionResult YorumEkle()
        {
            return View();
        }

        [HttpPost]
        [HttpPost]
        public IActionResult YorumEkle(KullaniciYorumlari model)
        {
            var yeniYorum = new KullaniciYorumlari
            {
                KullaniciId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                YorumMetni = model.YorumMetni,
                Puan = model.Puan,
                OlusturulmaTarihi = DateTime.Now
            };

            _context.KullaniciYorumlari.Add(yeniYorum);
            _context.SaveChanges();

            return RedirectToAction("GöstergePaneli");
           
        }




    }
}
