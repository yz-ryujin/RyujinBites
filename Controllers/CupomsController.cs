using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RyujinBites.Data;
using RyujinBites.Models.Lanchonete;

namespace RyujinBites.Controllers
{
    public class CupomsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CupomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cupoms
        public async Task<IActionResult> Index()
        {
            return View(await _context.Cupons.ToListAsync());
        }

        // GET: Cupoms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cupom = await _context.Cupons
                .FirstOrDefaultAsync(m => m.CupomId == id);
            if (cupom == null)
            {
                return NotFound();
            }

            return View(cupom);
        }

        // GET: Cupoms/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cupoms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CupomId,Codigo,TipoDesconto,ValorDesconto,DataInicio,DataFim,Ativo,UsosMaximos")] Cupom cupom)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cupom);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cupom);
        }

        // GET: Cupoms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cupom = await _context.Cupons.FindAsync(id);
            if (cupom == null)
            {
                return NotFound();
            }
            return View(cupom);
        }

        // POST: Cupoms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CupomId,Codigo,TipoDesconto,ValorDesconto,DataInicio,DataFim,Ativo,UsosMaximos")] Cupom cupom)
        {
            if (id != cupom.CupomId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cupom);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CupomExists(cupom.CupomId))
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
            return View(cupom);
        }

        // GET: Cupoms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cupom = await _context.Cupons
                .FirstOrDefaultAsync(m => m.CupomId == id);
            if (cupom == null)
            {
                return NotFound();
            }

            return View(cupom);
        }

        // POST: Cupoms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cupom = await _context.Cupons.FindAsync(id);
            if (cupom != null)
            {
                _context.Cupons.Remove(cupom);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CupomExists(int id)
        {
            return _context.Cupons.Any(e => e.CupomId == id);
        }
    }
}
