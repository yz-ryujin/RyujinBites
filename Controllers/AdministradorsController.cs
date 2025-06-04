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
    public class AdministradorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdministradorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Administradors
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Administradores.Include(a => a.ApplicationUser);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Administradors/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var administrador = await _context.Administradores
                .Include(a => a.ApplicationUser)
                .FirstOrDefaultAsync(m => m.AdministradorId == id);
            if (administrador == null)
            {
                return NotFound();
            }

            return View(administrador);
        }

        // GET: Administradors/Create
        public IActionResult Create()
        {
            ViewData["AdministradorId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Administradors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AdministradorId,Cargo,DataContratacao")] Administrador administrador)
        {
            if (ModelState.IsValid)
            {
                _context.Add(administrador);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AdministradorId"] = new SelectList(_context.Users, "Id", "Id", administrador.AdministradorId);
            return View(administrador);
        }

        // GET: Administradors/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var administrador = await _context.Administradores.FindAsync(id);
            if (administrador == null)
            {
                return NotFound();
            }
            ViewData["AdministradorId"] = new SelectList(_context.Users, "Id", "Id", administrador.AdministradorId);
            return View(administrador);
        }

        // POST: Administradors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("AdministradorId,Cargo,DataContratacao")] Administrador administrador)
        {
            if (id != administrador.AdministradorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(administrador);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdministradorExists(administrador.AdministradorId))
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
            ViewData["AdministradorId"] = new SelectList(_context.Users, "Id", "Id", administrador.AdministradorId);
            return View(administrador);
        }

        // GET: Administradors/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var administrador = await _context.Administradores
                .Include(a => a.ApplicationUser)
                .FirstOrDefaultAsync(m => m.AdministradorId == id);
            if (administrador == null)
            {
                return NotFound();
            }

            return View(administrador);
        }

        // POST: Administradors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var administrador = await _context.Administradores.FindAsync(id);
            if (administrador != null)
            {
                _context.Administradores.Remove(administrador);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AdministradorExists(string id)
        {
            return _context.Administradores.Any(e => e.AdministradorId == id);
        }
    }
}
