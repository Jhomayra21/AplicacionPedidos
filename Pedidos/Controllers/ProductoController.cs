using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Pedidos.Data;
using Pedidos.Models;

namespace Pedidos.Controllers
{
    public class ProductoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Producto
        public async Task<ActionResult> Index()
        {
            try
            {
                var productos = await _context.Productos.ToListAsync();
                return View(productos);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar los productos: " + ex.Message;
                return View(new List<ProductoModel>());
            }
        }

        // GET: Producto/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var producto = await _context.Productos
                    .FirstOrDefaultAsync(m => m.Id == id);
                
                if (producto == null)
                {
                    return NotFound();
                }

                return View(producto);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar el producto: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Producto/Create
        [Authorize(Policy = "EmployeeAccess")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Producto/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EmployeeAccess")]
        public async Task<ActionResult> Create([Bind("Nombre,Descripcion,Precio,Stock")] ProductoModel producto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Productos
                        .FirstOrDefaultAsync(p => p.Nombre.ToLower() == producto.Nombre.ToLower());
                    
                    if (existingProduct != null)
                    {
                        ModelState.AddModelError("Nombre", "Ya existe un producto con este nombre");
                        return View(producto);
                    }

                    _context.Add(producto);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Producto creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear el producto: " + ex.Message);
                }
            }
            return View(producto);
        }

        // GET: Producto/Edit/5
        [Authorize(Policy = "EmployeeAccess")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                {
                    return NotFound();
                }
                return View(producto);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar el producto: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EmployeeAccess")]
        public async Task<ActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion,Precio,Stock")] ProductoModel producto)
        {
            if (id != producto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Productos
                        .FirstOrDefaultAsync(p => p.Nombre.ToLower() == producto.Nombre.ToLower() && p.Id != producto.Id);
                    
                    if (existingProduct != null)
                    {
                        ModelState.AddModelError("Nombre", "Ya existe otro producto con este nombre");
                        return View(producto);
                    }

                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Producto actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar el producto: " + ex.Message);
                }
            }
            return View(producto);
        }

        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var producto = await _context.Productos
                    .FirstOrDefaultAsync(m => m.Id == id);
                
                if (producto == null)
                {
                    return NotFound();
                }

                return View(producto);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar el producto: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto != null)
                {
                    var isUsedInOrder = await _context.OrdenItems
                        .AnyAsync(oi => oi.ProductoId == id);
                    
                    if (isUsedInOrder)
                    {
                        TempData["Error"] = "No se puede eliminar el producto porque está siendo usado en órdenes existentes";
                        return RedirectToAction(nameof(Index));
                    }

                    _context.Productos.Remove(producto);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Producto eliminado exitosamente";
                }
                else
                {
                    TempData["Error"] = "Producto no encontrado";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el producto: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<ActionResult> Search(string searchString, decimal? minPrice, decimal? maxPrice)
        {
            try
            {
                var productos = _context.Productos.AsQueryable();

                if (!string.IsNullOrEmpty(searchString))
                {
                    productos = productos.Where(p => p.Nombre.Contains(searchString) || 
                                                   p.Descripcion.Contains(searchString));
                }

                if (minPrice.HasValue)
                {
                    productos = productos.Where(p => p.Precio >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    productos = productos.Where(p => p.Precio <= maxPrice.Value);
                }

                var result = await productos.ToListAsync();
                
                ViewBag.SearchString = searchString;
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;
                
                return View("Index", result);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error en la búsqueda: " + ex.Message;
                return View("Index", new List<ProductoModel>());
            }
        }
    }
}