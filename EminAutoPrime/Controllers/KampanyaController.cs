using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EminAutoPrime.Data;
using EminAutoPrime.Models;
using System.IO;

namespace EminAutoPrime.Controllers
{
    public class KampanyaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public KampanyaController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Kampanya
        public async Task<IActionResult> Index()
        {
            var kampanyalar = await _context.Kampanyalar                
                .OrderBy(k => k.BaslangicTarihi)
                .ToListAsync();
            return View(kampanyalar);
        }

        // GET: Kampanya/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kampanya = await _context.Kampanyalar
                .FirstOrDefaultAsync(k => k.KampanyaID == id);

            if (kampanya == null)
            {
                return NotFound();
            }

            return View(kampanya);
        }

        // GET: Kampanya/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Kampanya/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Kampanya kampanya, IFormFile resimDosyasi)
        {
            try
            {               
                
                    if (resimDosyasi != null && resimDosyasi.Length > 0)
                    {
                        // Resim dosyasını byte array'e dönüştürme
                        using (var memoryStream = new MemoryStream())
                        {
                            await resimDosyasi.CopyToAsync(memoryStream);
                            kampanya.GorselVerisi = memoryStream.ToArray();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("ResimDosyasi", "Lütfen bir resim dosyası seçin.");
                        return View(kampanya);
                    }

                    _context.Add(kampanya);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                ModelState.AddModelError("", "Bir hata oluştu, lütfen tekrar deneyin.");
            }

            return View(kampanya);
        }


        // GET: Kampanya/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kampanya = await _context.Kampanyalar.FindAsync(id);
            if (kampanya == null)
            {
                return NotFound();
            }
            return View(kampanya);
        }

        // POST: Kampanya/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Kampanya kampanya, IFormFile? resimDosyasi)
        {
            if (id != kampanya.KampanyaID)
            {
                return NotFound();
            }
            try
            {                
                var existingKampanya = await _context.Kampanyalar.FindAsync(id);
                if (existingKampanya == null)
                {
                    return NotFound();
                }
                
                existingKampanya.KampanyaBasligi = kampanya.KampanyaBasligi;
                existingKampanya.KampanyaAciklamasi = kampanya.KampanyaAciklamasi;
                existingKampanya.BaslangicTarihi = kampanya.BaslangicTarihi;
                existingKampanya.BitisTarihi = kampanya.BitisTarihi;
                                
                if (resimDosyasi != null && resimDosyasi.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await resimDosyasi.CopyToAsync(memoryStream);
                        existingKampanya.GorselVerisi = memoryStream.ToArray();
                    }
                }
                                
                _context.Update(existingKampanya);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KampanyaExists(kampanya.KampanyaID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return View(kampanya);
        }

        // GET: Kampanya/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kampanya = await _context.Kampanyalar
                .FirstOrDefaultAsync(m => m.KampanyaID == id);
            if (kampanya == null)
            {
                return NotFound();
            }

            return View(kampanya);
        }

        // POST: Kampanya/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kampanya = await _context.Kampanyalar.FindAsync(id);
            if (kampanya != null)
            {
                _context.Kampanyalar.Remove(kampanya);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KampanyaExists(int id)
        {
            return _context.Kampanyalar.Any(e => e.KampanyaID == id);
        }
    }
}
