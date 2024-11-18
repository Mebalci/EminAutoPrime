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
            return View(await _context.Kampanyalar.ToListAsync());
        }

        // GET: Kampanya/Details/5
        public async Task<IActionResult> Details(int? id)
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
        public async Task<IActionResult> Create(Kampanya kampanya)
        {
            if (ModelState.IsValid)
            {
                // Resim dosyası yüklendi mi kontrol edin
                if (kampanya.ResimDosyasi != null && kampanya.ResimDosyasi.Length > 0)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string uploadsFolder = Path.Combine(wwwRootPath, "uploads");

                    // uploads klasörünü oluşturun eğer yoksa
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string fileName = Path.GetFileNameWithoutExtension(kampanya.ResimDosyasi.FileName);
                    string extension = Path.GetExtension(kampanya.ResimDosyasi.FileName);
                    fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                    string path = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await kampanya.ResimDosyasi.CopyToAsync(fileStream);
                    }

                    kampanya.GorselYolu = "/uploads/" + fileName;
                }
                else
                {
                    ModelState.AddModelError("ResimDosyasi", "Lütfen bir resim dosyası yükleyin.");
                    return View(kampanya);
                }

                _context.Add(kampanya);
                await _context.SaveChangesAsync();
                             
                return RedirectToAction("Index");
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
        public async Task<IActionResult> Edit(int id, [Bind("KampanyaID,KampanyaBasligi,KampanyaAciklamasi,BaslangicTarihi,BitisTarihi,GorselYolu")] Kampanya kampanya)
        {
            if (id != kampanya.KampanyaID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(kampanya);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
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
