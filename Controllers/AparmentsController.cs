using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoMLHOMP.Models;

namespace ProyectoMLHOMP.Controllers
{
    public class AparmentsController : Controller
    {
        private readonly DataProyecto _context;

        public AparmentsController(DataProyecto context)
        {
            _context = context;
        }

        // GET: Aparments
        public async Task<IActionResult> Index()
        {
            return View(await _context.Aparment.ToListAsync());
        }

        // GET: Aparments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aparment = await _context.Aparment
                .FirstOrDefaultAsync(m => m.Id == id);
            if (aparment == null)
            {
                return NotFound();
            }

            return View(aparment);
        }

        // GET: Aparments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Aparments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,PricePerNight,Address,City,Country,Bedrooms,Bathrooms,MaxOccupancy,IsAvailable,CreatedAt,UpdatedAt")] Aparment aparment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(aparment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(aparment);
        }

        // GET: Aparments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aparment = await _context.Aparment.FindAsync(id);
            if (aparment == null)
            {
                return NotFound();
            }
            return View(aparment);
        }

        // POST: Aparments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,PricePerNight,Address,City,Country,Bedrooms,Bathrooms,MaxOccupancy,IsAvailable,CreatedAt,UpdatedAt")] Aparment aparment)
        {
            if (id != aparment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(aparment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AparmentExists(aparment.Id))
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
            return View(aparment);
        }

        // GET: Aparments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aparment = await _context.Aparment
                .FirstOrDefaultAsync(m => m.Id == id);
            if (aparment == null)
            {
                return NotFound();
            }

            return View(aparment);
        }

        // POST: Aparments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aparment = await _context.Aparment.FindAsync(id);
            if (aparment != null)
            {
                _context.Aparment.Remove(aparment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AparmentExists(int id)
        {
            return _context.Aparment.Any(e => e.Id == id);
        }
    }
}
