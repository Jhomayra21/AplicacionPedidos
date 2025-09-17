using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos.Data;
using Pedidos.Models;

namespace Pedidos.Controllers
{
    public class OrdenController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdenController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Orden
        public async Task<ActionResult> Index()
        {
            try
            {
                var ordenes = await _context.Ordenes
                    .Include(o => o.Cliente)
                    .Include(o => o.Items)
                    .ToListAsync();
                return View(ordenes);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar las órdenes: " + ex.Message;
                return View(new List<OrdenModel>());
            }
        }

        // GET: Orden/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var orden = await _context.Ordenes
                    .Include(o => o.Cliente)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Producto)
                    .FirstOrDefaultAsync(m => m.Id == id);
                
                if (orden == null)
                {
                    return NotFound();
                }

                // Cargar productos disponibles para agregar a la orden
                ViewBag.ProductosDisponibles = await _context.Productos
                    .Where(p => p.Stock > 0)
                    .ToListAsync();

                return View(orden);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar la orden: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Orden/Create
        public async Task<ActionResult> Create()
        {
            try
            {
                ViewBag.Clientes = await _context.Users
                    .Where(u => u.Rol == "cliente")
                    .ToListAsync();
                ViewBag.Productos = await _context.Productos
                    .Where(p => p.Stock > 0)
                    .ToListAsync();
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar datos: " + ex.Message;
                return View();
            }
        }

        // POST: Orden/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("ClienteId,Estado")] OrdenModel orden)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    orden.Fecha = DateTime.Now;
                    orden.Total = 0; // Se calculará al agregar items
                    
                    _context.Add(orden);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Orden creada exitosamente. Ahora puede agregar productos.";
                    return RedirectToAction(nameof(Details), new { id = orden.Id });
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al crear la orden: " + ex.Message);
            }

            // Recargar datos si hay error
            ViewBag.Clientes = await _context.Users
                .Where(u => u.Rol == "cliente")
                .ToListAsync();
            ViewBag.Productos = await _context.Productos
                .Where(p => p.Stock > 0)
                .ToListAsync();
            return View(orden);
        }

        // GET: Orden/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var orden = await _context.Ordenes.FindAsync(id);
                if (orden == null)
                {
                    return NotFound();
                }

                ViewBag.Clientes = await _context.Users
                    .Where(u => u.Rol == "cliente")
                    .ToListAsync();
                return View(orden);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar la orden: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Orden/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, [Bind("Id,ClienteId,Fecha,Estado,Total")] OrdenModel orden)
        {
            if (id != orden.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orden);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Orden actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrdenExists(orden.Id))
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
                    ModelState.AddModelError("", "Error al actualizar la orden: " + ex.Message);
                }
            }

            ViewBag.Clientes = await _context.Users
                .Where(u => u.Rol == "cliente")
                .ToListAsync();
            return View(orden);
        }

        // GET: Orden/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var orden = await _context.Ordenes
                    .Include(o => o.Cliente)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Producto)
                    .FirstOrDefaultAsync(m => m.Id == id);
                
                if (orden == null)
                {
                    return NotFound();
                }

                return View(orden);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar la orden: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Orden/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var orden = await _context.Ordenes
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == id);
                
                if (orden != null)
                {
                    // Restaurar stock de productos
                    foreach (var item in orden.Items)
                    {
                        var producto = await _context.Productos.FindAsync(item.ProductoId);
                        if (producto != null)
                        {
                            producto.Stock += item.Cantidad;
                        }
                    }

                    _context.Ordenes.Remove(orden);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Orden eliminada exitosamente y stock restaurado";
                }
                else
                {
                    TempData["Error"] = "Orden no encontrada";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar la orden: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // Método para agregar productos a una orden
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AgregarProducto(int ordenId, int productoId, int cantidad)
        {
            try
            {
                var orden = await _context.Ordenes
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == ordenId);
                
                var producto = await _context.Productos.FindAsync(productoId);
                
                if (orden == null || producto == null)
                {
                    TempData["Error"] = "Orden o producto no encontrado";
                    return RedirectToAction(nameof(Details), new { id = ordenId });
                }

                if (producto.Stock < cantidad)
                {
                    TempData["Error"] = $"Stock insuficiente. Solo hay {producto.Stock} unidades disponibles";
                    return RedirectToAction(nameof(Details), new { id = ordenId });
                }

                // Verificar si el producto ya está en la orden
                var itemExistente = orden.Items.FirstOrDefault(i => i.ProductoId == productoId);
                
                if (itemExistente != null)
                {
                    // Actualizar cantidad existente
                    if (producto.Stock < (itemExistente.Cantidad + cantidad))
                    {
                        TempData["Error"] = $"Stock insuficiente. Solo hay {producto.Stock} unidades disponibles y ya tiene {itemExistente.Cantidad} en la orden";
                        return RedirectToAction(nameof(Details), new { id = ordenId });
                    }
                    
                    itemExistente.Cantidad += cantidad;
                    itemExistente.Subtotal = itemExistente.Cantidad * producto.Precio;
                }
                else
                {
                    // Crear nuevo item
                    var nuevoItem = new OrdenItem
                    {
                        OrdenId = ordenId,
                        ProductoId = productoId,
                        Cantidad = cantidad,
                        Subtotal = cantidad * producto.Precio
                    };
                    
                    _context.OrdenItems.Add(nuevoItem);
                }

                // Reducir stock
                producto.Stock -= cantidad;
                
                // Recalcular total de la orden
                await _context.SaveChangesAsync();
                await RecalcularTotal(ordenId);
                
                TempData["Success"] = "Producto agregado exitosamente";
                return RedirectToAction(nameof(Details), new { id = ordenId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al agregar producto: " + ex.Message;
                return RedirectToAction(nameof(Details), new { id = ordenId });
            }
        }

        private async Task RecalcularTotal(int ordenId)
        {
            var orden = await _context.Ordenes
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == ordenId);
            
            if (orden != null)
            {
                orden.Total = orden.Items.Sum(i => i.Subtotal);
                await _context.SaveChangesAsync();
            }
        }

        private bool OrdenExists(int id)
        {
            return _context.Ordenes.Any(e => e.Id == id);
        }
    }
}
