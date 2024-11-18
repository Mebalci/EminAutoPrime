using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EminAutoPrime.Data;
using EminAutoPrime.Models;

namespace EminAutoPrime.Controllers
{
    public class EminAutoServisController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EminAutoServisController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: EminAutoServis
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.EminAutoServisler.Include(e => e.Arac);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: EminAutoServis/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eminAutoServis = await _context.EminAutoServisler
                .Include(e => e.Arac)
                .FirstOrDefaultAsync(m => m.ServisId == id);
            if (eminAutoServis == null)
            {
                return NotFound();
            }

            return View(eminAutoServis);
        }

        // GET: EminAutoServis/Create
        public IActionResult Create()
        {
            ViewData["AracId"] = new SelectList(_context.EminAutoAraclar, "AracId", "Marka");
            return View();
        }

        // POST: EminAutoServis/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ServisId,AracId,YapilanIslemler,GirisTarihi,CikisTarihi,Tamamlandi")] EminAutoServis eminAutoServis)
        {
            if (ModelState.IsValid)
            {
                _context.Add(eminAutoServis);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AracId"] = new SelectList(_context.EminAutoAraclar, "AracId", "Marka", eminAutoServis.AracId);
            return View(eminAutoServis);
        }

        // GET: EminAutoServis/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eminAutoServis = await _context.EminAutoServisler.FindAsync(id);
            if (eminAutoServis == null)
            {
                return NotFound();
            }
            ViewData["AracId"] = new SelectList(_context.EminAutoAraclar, "AracId", "Marka", eminAutoServis.AracId);
            return View(eminAutoServis);
        }

        // POST: EminAutoServis/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ServisId,AracId,YapilanIslemler,GirisTarihi,CikisTarihi,Tamamlandi")] EminAutoServis eminAutoServis)
        {
            if (id != eminAutoServis.ServisId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eminAutoServis);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EminAutoServisExists(eminAutoServis.ServisId))
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
            ViewData["AracId"] = new SelectList(_context.EminAutoAraclar, "AracId", "Marka", eminAutoServis.AracId);
            return View(eminAutoServis);
        }

        // GET: EminAutoServis/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eminAutoServis = await _context.EminAutoServisler
                .Include(e => e.Arac)
                .FirstOrDefaultAsync(m => m.ServisId == id);
            if (eminAutoServis == null)
            {
                return NotFound();
            }

            return View(eminAutoServis);
        }

        // POST: EminAutoServis/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eminAutoServis = await _context.EminAutoServisler.FindAsync(id);
            if (eminAutoServis != null)
            {
                _context.EminAutoServisler.Remove(eminAutoServis);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EminAutoServisExists(int id)
        {
            return _context.EminAutoServisler.Any(e => e.ServisId == id);
        }
    }
}
