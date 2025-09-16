using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Pedidos.Controllers
{
    public class OrdenController : Controller
    {
        // GET: OrdenController
        public ActionResult Index()
        {
            return View();
        }

        // GET: OrdenController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: OrdenController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: OrdenController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: OrdenController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: OrdenController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: OrdenController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: OrdenController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
