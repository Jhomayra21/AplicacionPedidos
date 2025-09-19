using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Pedidos.Data;
using Pedidos.Models;

namespace Pedidos.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UserController
        public async Task<ActionResult> Index()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                return View(users);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar los usuarios: " + ex.Message;
                return View(new List<UserModel>());
            }
        }

        // GET: UserController/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(m => m.Id == id);
                
                if (user == null)
                {
                    return NotFound();
                }

                return View(user);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar el usuario: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: UserController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UserController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("Nombre,Email,Password,Rol")] UserModel user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == user.Email);
                    
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Email", "Este email ya está en uso");
                        return View(user);
                    }

                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Usuario creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear el usuario: " + ex.Message);
                }
            }
            return View(user);
        }

        // GET: UserController/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
                return View(user);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar el usuario: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: UserController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, [Bind("Id,Nombre,Email,Password,Rol")] UserModel user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == user.Email && u.Id != id);
                    
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Email", "Este email ya está en uso por otro usuario");
                        return View(user);
                    }
                    
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Usuario actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar el usuario: " + ex.Message);
                }
            }
            return View(user);
        }

        // GET: UserController/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(m => m.Id == id);
                
                if (user == null)
                {
                    return NotFound();
                }

                return View(user);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar el usuario: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: UserController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user != null)
                {
                    var tieneOrdenes = await _context.Ordenes.AnyAsync(o => o.ClienteId == id);
                    if (tieneOrdenes)
                    {
                        TempData["Error"] = "No se puede eliminar el usuario porque tiene órdenes asociadas";
                        return RedirectToAction(nameof(Index));
                    }
                    
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Usuario eliminado exitosamente";
                }
                else
                {
                    TempData["Error"] = "Usuario no encontrado";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el usuario: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
