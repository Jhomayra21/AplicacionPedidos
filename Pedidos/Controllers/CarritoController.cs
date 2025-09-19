using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Pedidos.Data;
using Pedidos.Models;
using System.Security.Claims;

namespace Pedidos.Controllers
{
    [Authorize(Policy = "ClientAccess")]
    public class CarritoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarritoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var clienteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var carrito = await _context.Ordenes
                .Include(o => o.Cliente)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Producto)
                .FirstOrDefaultAsync(o => o.ClienteId == clienteId && o.Estado == "Carrito");

            if (carrito == null)
            {
                carrito = new OrdenModel
                {
                    ClienteId = clienteId,
                    Fecha = DateTime.Now,
                    Estado = "Carrito",
                    Total = 0
                };

                _context.Ordenes.Add(carrito);
                await _context.SaveChangesAsync();

                carrito = await _context.Ordenes
                    .Include(o => o.Cliente)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Producto)
                    .FirstOrDefaultAsync(o => o.Id == carrito.Id);
            }
            ViewBag.ProductosDisponibles = await _context.Productos
                .Where(p => p.Stock > 0)
                .ToListAsync();

            return View(carrito);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarProducto(int productoId, int cantidad)
        {
            var clienteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var carrito = await _context.Ordenes
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.ClienteId == clienteId && o.Estado == "Carrito");

            if (carrito == null)
            {
                carrito = new OrdenModel
                {
                    ClienteId = clienteId,
                    Fecha = DateTime.Now,
                    Estado = "Carrito",
                    Total = 0
                };

                _context.Ordenes.Add(carrito);
                await _context.SaveChangesAsync();
            }

            var producto = await _context.Productos.FindAsync(productoId);
            if (producto == null)
            {
                TempData["Error"] = "El producto seleccionado no existe";
                return RedirectToAction(nameof(Index));
            }

            if (producto.Stock < cantidad)
            {
                TempData["Error"] = $"Stock insuficiente. Solo hay {producto.Stock} unidades disponibles";
                return RedirectToAction(nameof(Index));
            }

            var itemExistente = carrito.Items.FirstOrDefault(i => i.ProductoId == productoId);
            if (itemExistente != null)
            {
                if (producto.Stock < (itemExistente.Cantidad + cantidad))
                {
                    TempData["Error"] = $"Stock insuficiente. Solo hay {producto.Stock} unidades disponibles y ya tiene {itemExistente.Cantidad} en su carrito";
                    return RedirectToAction(nameof(Index));
                }

                itemExistente.Cantidad += cantidad;
                itemExistente.Subtotal = itemExistente.Cantidad * producto.Precio;
            }
            else
            {
                var nuevoItem = new OrdenItem
                {
                    OrdenId = carrito.Id,
                    ProductoId = productoId,
                    Cantidad = cantidad,
                    Subtotal = cantidad * producto.Precio
                };

                _context.OrdenItems.Add(nuevoItem);
            }
            await _context.SaveChangesAsync();
            carrito.Total = carrito.Items.Sum(i => i.Subtotal);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Producto agregado al carrito exitosamente";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarItem(int itemId)
        {
            var clienteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var item = await _context.OrdenItems
                .Include(i => i.Orden)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.Orden.ClienteId == clienteId && i.Orden.Estado == "Carrito");

            if (item == null)
            {
                TempData["Error"] = "El producto no existe en su carrito";
                return RedirectToAction(nameof(Index));
            }

            _context.OrdenItems.Remove(item);
            await _context.SaveChangesAsync();

            var carrito = await _context.Ordenes
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == item.OrdenId);

            if (carrito != null)
            {
                carrito.Total = carrito.Items.Sum(i => i.Subtotal);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Producto eliminado del carrito exitosamente";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarCantidad(int itemId, int cantidad)
        {
            if (cantidad <= 0)
            {
                TempData["Error"] = "La cantidad debe ser mayor a 0";
                return RedirectToAction(nameof(Index));
            }

            var clienteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var item = await _context.OrdenItems
                .Include(i => i.Orden)
                .Include(i => i.Producto)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.Orden.ClienteId == clienteId && i.Orden.Estado == "Carrito");

            if (item == null)
            {
                TempData["Error"] = "El producto no existe en su carrito";
                return RedirectToAction(nameof(Index));
            }

            if (item.Producto.Stock < cantidad)
            {
                TempData["Error"] = $"Stock insuficiente. Solo hay {item.Producto.Stock} unidades disponibles";
                return RedirectToAction(nameof(Index));
            }

            item.Cantidad = cantidad;
            item.Subtotal = cantidad * item.Producto.Precio;
            await _context.SaveChangesAsync();

            var carrito = await _context.Ordenes
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == item.OrdenId);

            if (carrito != null)
            {
                carrito.Total = carrito.Items.Sum(i => i.Subtotal);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Cantidad actualizada exitosamente";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Finalizar()
        {
            var clienteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var carrito = await _context.Ordenes
                .Include(o => o.Cliente)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Producto)
                .FirstOrDefaultAsync(o => o.ClienteId == clienteId && o.Estado == "Carrito");

            if (carrito == null || !carrito.Items.Any())
            {
                TempData["Error"] = "No hay productos en su carrito";
                return RedirectToAction(nameof(Index));
            }

            return View(carrito);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarPedido()
        {
            var clienteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var carrito = await _context.Ordenes
                .Include(o => o.Items)
                    .ThenInclude(i => i.Producto)
                .FirstOrDefaultAsync(o => o.ClienteId == clienteId && o.Estado == "Carrito");

            if (carrito == null || !carrito.Items.Any())
            {
                TempData["Error"] = "No hay productos en su carrito";
                return RedirectToAction(nameof(Index));
            }
            foreach (var item in carrito.Items)
            {
                if (item.Producto.Stock < item.Cantidad)
                {
                    TempData["Error"] = $"Stock insuficiente para {item.Producto.Nombre}. Solo hay {item.Producto.Stock} unidades disponibles";
                    return RedirectToAction(nameof(Finalizar));
                }
                item.Producto.Stock -= item.Cantidad;
            }

            carrito.Estado = "Pendiente";
            carrito.Fecha = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "¡Pedido confirmado exitosamente! Su orden está siendo procesada.";
            return RedirectToAction("MisPedidos");
        }

        public async Task<IActionResult> MisPedidos()
        {
            var clienteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var ordenes = await _context.Ordenes
                .Include(o => o.Items)
                    .ThenInclude(i => i.Producto)
                .Where(o => o.ClienteId == clienteId && o.Estado != "Carrito")
                .OrderByDescending(o => o.Fecha)
                .ToListAsync();

            return View(ordenes);
        }

        public async Task<IActionResult> DetallePedido(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clienteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var orden = await _context.Ordenes
                .Include(o => o.Items)
                    .ThenInclude(i => i.Producto)
                .FirstOrDefaultAsync(o => o.Id == id && o.ClienteId == clienteId);

            if (orden == null)
            {
                return NotFound();
            }

            return View(orden);
        }

        public async Task<IActionResult> CatalogoProductos(string searchString, decimal? minPrice, decimal? maxPrice)
        {
            var productos = _context.Productos.Where(p => p.Stock > 0).AsQueryable();

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

            ViewBag.SearchString = searchString;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(await productos.ToListAsync());
        }
    }
}