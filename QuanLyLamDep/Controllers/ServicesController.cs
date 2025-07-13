using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using QuanLyLamDep.Models;

namespace QuanLyLamDep.Controllers
{
    public class ServicesController : Controller
    {
        private BeautySalonDBEntities db = new BeautySalonDBEntities();

        // GET: Services
        public ActionResult Index()
        {
            var services = db.Services.Include(s => s.ServiceGroup);
            return View(services.ToList());
        }

        // GET: Services/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Service service = db.Services.Find(id);
            if (service == null)
            {
                return HttpNotFound();
            }
            return View(service);
        }

        // GET: Services/Create
        public ActionResult Create()
        {
            ViewBag.ServiceGroupID = new SelectList(db.ServiceGroups, "ServiceGroupID", "Name");
            return View();
        }

        // POST: Services/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ServiceID,ServiceGroupID,Name,Description,Price,Duration,Status,ImageUrl")] Service service)
        {
            if (ModelState.IsValid)
            {
                db.Services.Add(service);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ServiceGroupID = new SelectList(db.ServiceGroups, "ServiceGroupID", "Name", service.ServiceGroupID);
            return View(service);
        }

        // GET: Services/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Service service = db.Services.Find(id);
            if (service == null)
            {
                return HttpNotFound();
            }
            ViewBag.ServiceGroupID = new SelectList(db.ServiceGroups, "ServiceGroupID", "Name", service.ServiceGroupID);
            return View(service);
        }

        // POST: Services/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ServiceID,ServiceGroupID,Name,Description,Price,Duration,Status,ImageUrl")] Service service)
        {
            if (ModelState.IsValid)
            {
                db.Entry(service).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ServiceGroupID = new SelectList(db.ServiceGroups, "ServiceGroupID", "Name", service.ServiceGroupID);
            return View(service);
        }

        // GET: Services/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Service service = db.Services.Find(id);
            if (service == null)
            {
                return HttpNotFound();
            }
            return View(service);
        }

        // POST: Services/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Service service = db.Services.Find(id);
            db.Services.Remove(service);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        [HttpPost]
        public ActionResult BookMultipleServices(string selectedServices)
        {
            if (string.IsNullOrEmpty(selectedServices))
            {
                TempData["Error"] = "Bạn chưa chọn dịch vụ nào!";
                return RedirectToAction("Index");
            }
            var serviceItems = selectedServices.Split(',')
                .Select(s => s.Split(':'))
                .Where(parts => parts.Length == 2)
                .Select(parts => new
                {
                    ServiceID = int.Parse(parts[0]),
                    Quantity = int.Parse(parts[1])
                }).ToList();

            // 🛠️ Tách riêng danh sách ID để dùng trong Contains
            var serviceIds = serviceItems.Select(x => x.ServiceID).ToList();

            var selectedList = db.Services
                .Where(s => serviceIds.Contains(s.ServiceID))
                .ToList();


            // Gắn Quantity vào từng dịch vụ để truyền sang View
            var servicesWithQuantity = selectedList
                .Select(s => new Service
                {
                    ServiceID = s.ServiceID,
                    Name = s.Name,
                    Description = s.Description,
                    Price = s.Price,
                    Duration = s.Duration,
                    Status = s.Status,
                    ImageUrl = s.ImageUrl,
                    ServiceGroup = s.ServiceGroup
                    // Quantity được gắn thông qua ViewBag
                }).ToList();

            ViewBag.ServiceQuantities = serviceItems.ToDictionary(x => x.ServiceID, x => x.Quantity);
            ViewBag.SelectedTotal = serviceItems.Sum(x =>
            {
                var matched = selectedList.FirstOrDefault(s => s.ServiceID == x.ServiceID);
                return matched != null ? x.Quantity * matched.Price : 0;
            });

            return View("Checkout", servicesWithQuantity);
        }


    }
}
