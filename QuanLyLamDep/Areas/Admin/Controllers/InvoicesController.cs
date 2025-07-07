    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;
    using QuanLyLamDep.Models;

    namespace QuanLyLamDep.Areas.Admin.Controllers
    {
        public class InvoicesController : Controller
        {
            private BeautySalonDBEntities db = new BeautySalonDBEntities();

            // GET: Admin/Invoices
            public ActionResult Index()
            {
                var invoices = db.Invoices.Include(i => i.Customer).Include(i => i.Employee);
                return View(invoices.ToList());
            }

        // GET: Admin/Invoices/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Load Invoice cùng các dữ liệu liên quan: Customer, Employee, InvoiceDetails + Service,
            // ProductSales + Product, Payments
            var invoice = db.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Employee)
                .Include(i => i.InvoiceDetails.Select(d => d.Service))
                .Include(i => i.ProductSales.Select(ps => ps.Product))
                .Include(i => i.Payments)
                .FirstOrDefault(i => i.InvoiceID == id);

            if (invoice == null)
            {
                return HttpNotFound();
            }

            return View(invoice);
        }


        // GET: Admin/Invoices/Create
        public ActionResult Create()
            {
                ViewBag.CustomerID = new SelectList(db.Customers, "CustomerID", "FullName");
                ViewBag.EmployeeID = new SelectList(db.Employees, "EmployeeID", "FullName");
                return View();
            }

        // POST: Admin/Invoices/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "InvoiceID,CustomerID,CreatedDate,TotalAmount,PaymentMethod,PaymentStatus,EmployeeID")] Invoice invoice)
        {
            if (invoice.TotalAmount < 1000) // kiểm tra số tiền
            {
                ModelState.AddModelError("TotalAmount", "Số tiền phải lớn hơn hoặc bằng 1,000.");
            }

            if (ModelState.IsValid)
            {
                db.Invoices.Add(invoice);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CustomerID = new SelectList(db.Customers, "CustomerID", "FullName", invoice.CustomerID);
            ViewBag.EmployeeID = new SelectList(db.Employees, "EmployeeID", "FullName", invoice.EmployeeID);
            return View(invoice);
        }


        // GET: Admin/Invoices/Edit/5
        public ActionResult Edit(int? id)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Invoice invoice = db.Invoices.Find(id);
                if (invoice == null)
                {
                    return HttpNotFound();
                }
                ViewBag.CustomerID = new SelectList(db.Customers, "CustomerID", "FullName", invoice.CustomerID);
                ViewBag.EmployeeID = new SelectList(db.Employees, "EmployeeID", "FullName", invoice.EmployeeID);
                return View(invoice);
            }

            // POST: Admin/Invoices/Edit/5
            // To protect from overposting attacks, enable the specific properties you want to bind to, for 
            // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Edit([Bind(Include = "InvoiceID,CustomerID,CreatedDate,TotalAmount,PaymentMethod,PaymentStatus,EmployeeID")] Invoice invoice)
            {
                if (ModelState.IsValid)
                {
                    db.Entry(invoice).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                ViewBag.CustomerID = new SelectList(db.Customers, "CustomerID", "FullName", invoice.CustomerID);
                ViewBag.EmployeeID = new SelectList(db.Employees, "EmployeeID", "FullName", invoice.EmployeeID);
                return View(invoice);
            }

            // GET: Admin/Invoices/Delete/5
            public ActionResult Delete(int? id)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Invoice invoice = db.Invoices.Find(id);
                if (invoice == null)
                {
                    return HttpNotFound();
                }
                return View(invoice);
            }

            // POST: Admin/Invoices/Delete/5
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public ActionResult DeleteConfirmed(int id)
            {
                Invoice invoice = db.Invoices.Find(id);
                db.Invoices.Remove(invoice);
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
        }
    }
