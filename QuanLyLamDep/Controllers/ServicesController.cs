using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using QuanLyLamDep.Models;
using QuanLyLamDep.Models.ViewModels;

namespace QuanLyLamDep.Controllers
{
    public class ServiceController : Controller
    {
        private BeautySalonDBEntities db = new BeautySalonDBEntities();

        public ActionResult Index()
        {
            var services = db.Services.Include(s => s.ServiceGroup).ToList();
            ViewBag.ServiceGroups = db.ServiceGroups.ToList();
            return View(services);
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Service service = db.Services.Include(s => s.ServiceGroup).FirstOrDefault(s => s.ServiceID == id);
            if (service == null) return HttpNotFound();

            // Lấy đánh giá từ Session
            string key = "ServiceReviews_" + id;
            var reviews = Session[key] as List<ServiceReviewVM> ?? new List<ServiceReviewVM>();
            ViewBag.Reviews = reviews;

            return View(service);
        }

        [HttpPost]
        public ActionResult SubmitReview(int serviceId, string userName, int rating, string comment)
        {
            var review = new ServiceReviewVM
            {
                UserName = userName,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.Now
            };

            string key = "ServiceReviews_" + serviceId;
            var reviews = Session[key] as List<ServiceReviewVM> ?? new List<ServiceReviewVM>();
            reviews.Add(review);
            Session[key] = reviews;

            TempData["ReviewSuccess"] = "Cảm ơn bạn đã đánh giá!";
            return RedirectToAction("Details", new { id = serviceId });
        }

        public ActionResult Create()
        {
            ViewBag.ServiceGroupID = new SelectList(db.ServiceGroups, "ServiceGroupID", "Name");
            return View();
        }

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

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Service service = db.Services.Find(id);
            if (service == null) return HttpNotFound();

            ViewBag.ServiceGroupID = new SelectList(db.ServiceGroups, "ServiceGroupID", "Name", service.ServiceGroupID);
            return View(service);
        }

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

        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Service service = db.Services.Find(id);
            if (service == null) return HttpNotFound();

            return View(service);
        }

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
            if (disposing) db.Dispose();
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

            var serviceIds = serviceItems.Select(x => x.ServiceID).ToList();

            var selectedList = db.Services
                .Where(s => serviceIds.Contains(s.ServiceID))
                .ToList();

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
