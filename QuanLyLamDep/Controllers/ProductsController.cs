using QuanLyLamDep.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace QuanLyLamDep.Controllers
{
    public class ProductsController : Controller
    {
        private BeautySalonDBEntities db = new BeautySalonDBEntities();

        // GET: Products
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.Category);
            return View(products.ToList());
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            return View(product);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "Name");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,CategoryID,Name,Description,Unit,UnitPrice,ImportDate,ImageUrl")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "Name", product.CategoryID);
            return View(product);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "Name", product.CategoryID);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,CategoryID,Name,Description,Unit,UnitPrice,ImportDate,ImageUrl")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "Name", product.CategoryID);
            return View(product);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // ✅ POST: Products/Checkout
        [HttpPost]
        public ActionResult Checkout(string selectedProducts)
        {
            if (string.IsNullOrEmpty(selectedProducts))
            {
                TempData["Error"] = "Bạn chưa chọn sản phẩm nào!";
                return RedirectToAction("Index");
            }

            var productItems = selectedProducts.Split(',')
                .Select(s => s.Split(':'))
                .Where(parts => parts.Length == 2)
                .Select(parts => new
                {
                    ProductID = int.Parse(parts[0]),
                    Quantity = int.Parse(parts[1])
                }).ToList();

            var productIds = productItems.Select(x => x.ProductID).ToList();

            var selectedList = db.Products
                .Where(p => productIds.Contains(p.ProductID))
                .Include(p => p.Category)
                .ToList();

            var productsWithQuantity = selectedList
                .Select(p => new Product
                {
                    ProductID = p.ProductID,
                    Name = p.Name,
                    Description = p.Description,
                    Unit = p.Unit,
                    UnitPrice = p.UnitPrice,
                    ImportDate = p.ImportDate,
                    ImageUrl = p.ImageUrl,
                    Category = p.Category
                }).ToList();

            ViewBag.ProductQuantities = productItems.ToDictionary(x => x.ProductID, x => x.Quantity);
            ViewBag.SelectedTotal = productItems.Sum(x =>
            {
                var matched = selectedList.FirstOrDefault(p => p.ProductID == x.ProductID);
                return matched != null ? x.Quantity * matched.UnitPrice : 0;
            });

            return View("Checkout", productsWithQuantity);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
