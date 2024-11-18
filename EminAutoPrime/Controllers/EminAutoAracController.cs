using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EminAutoPrime.Data;
using EminAutoPrime.Models;
using Microsoft.AspNetCore.Authorization;

namespace EminAutoPrime.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EminAutoAracController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EminAutoAracController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: EminAutoAracs
        public async Task<IActionResult> Index()
        {
            var araclar = await _context.EminAutoAraclar.ToListAsync();
            return View(araclar);
        }

        // GET: EminAutoAracs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eminAutoArac = await _context.EminAutoAraclar
                .FirstOrDefaultAsync(m => m.AracId == id);
            if (eminAutoArac == null)
            {
                return NotFound();
            }

            return View(eminAutoArac);
        }

        // GET: EminAutoAracs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EminAutoAracs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AracId,Marka,Model,Yil,Plaka,SahipAdi")] EminAutoArac eminAutoArac)
        {
            if (ModelState.IsValid)
            {
                _context.Add(eminAutoArac);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(eminAutoArac);
        }

        // GET: EminAutoAracs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eminAutoArac = await _context.EminAutoAraclar.FindAsync(id);
            if (eminAutoArac == null)
            {
                return NotFound();
            }
            return View(eminAutoArac);
        }

        // POST: EminAutoAracs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AracId,Marka,Model,Yil,Plaka,SahipAdi")] EminAutoArac eminAutoArac)
        {
            if (id != eminAutoArac.AracId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eminAutoArac);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EminAutoAracExists(eminAutoArac.AracId))
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
            return View(eminAutoArac);
        }

        // GET: EminAutoAracs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eminAutoArac = await _context.EminAutoAraclar
                .FirstOrDefaultAsync(m => m.AracId == id);
            if (eminAutoArac == null)
            {
                return NotFound();
            }

            return View(eminAutoArac);
        }

        // POST: EminAutoAracs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eminAutoArac = await _context.EminAutoAraclar.FindAsync(id);
            if (eminAutoArac != null)
            {
                _context.EminAutoAraclar.Remove(eminAutoArac);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EminAutoAracExists(int id)
        {
            return _context.EminAutoAraclar.Any(e => e.AracId == id);
        }
    }
}
