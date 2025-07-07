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
    public class InvoicesController : Controller
    {
        private BeautySalonDBEntities db = new BeautySalonDBEntities();

        // GET: Invoices
        public ActionResult Index()
        {
            var invoices = db.Invoices.Include(i => i.Customer).Include(i => i.Employee);
            return View(invoices.ToList());
        }

        // GET: Invoices/Details/5
        public ActionResult Details(int? id)
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

        // GET: Invoices/Create
        public ActionResult Create()
        {
            ViewBag.CustomerID = new SelectList(db.Customers, "CustomerID", "FullName");
            ViewBag.EmployeeID = new SelectList(db.Employees, "EmployeeID", "FullName");
            return View();
        }

        // POST: Invoices/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "InvoiceID,CustomerID,CreatedDate,TotalAmount,PaymentMethod,PaymentStatus,EmployeeID")] Invoice invoice)
        {
            if (ModelState.IsValid)
            {
                invoice.CreatedDate = invoice.CreatedDate ?? DateTime.Now;
                db.Invoices.Add(invoice);
                db.SaveChanges();
                return RedirectToAction("CreateInvoice", new { id = invoice.InvoiceID });
            }

            ViewBag.CustomerID = new SelectList(db.Customers, "CustomerID", "FullName", invoice.CustomerID);
            ViewBag.EmployeeID = new SelectList(db.Employees, "EmployeeID", "FullName", invoice.EmployeeID);
            return View(invoice);
        }

        // GET: Invoices/CreateInvoice/5
        public ActionResult CreateInvoice(int? id)
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
            var services = db.Services.ToList();
            var products = db.Products.Include(p => p.Category).ToList();
            ViewBag.Services = services;
            ViewBag.Products = products;
            ViewBag.Invoice = invoice;
            // Lấy giỏ hàng từ Session
            var serviceCart = Session["ServiceCart"] as List<InvoiceDetail> ?? new List<InvoiceDetail>();
            var productCart = Session["ProductCart"] as List<ProductSale> ?? new List<ProductSale>();
            ViewBag.ServiceCart = serviceCart;
            ViewBag.ProductCart = productCart;
            return View();
        }

        // POST: Invoices/CreateInvoice
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateInvoice(int invoiceId, string type, int itemId, int quantity)
        {
            var serviceCart = Session["ServiceCart"] as List<InvoiceDetail> ?? new List<InvoiceDetail>();
            var productCart = Session["ProductCart"] as List<ProductSale> ?? new List<ProductSale>();

            if (type == "service")
            {
                var service = db.Services.Find(itemId);
                if (service != null)
                {
                    var detail = new InvoiceDetail
                    {
                        InvoiceID = invoiceId,
                        ServiceID = itemId,
                        Quantity = quantity,
                        UnitPrice = service.Price
                    };
                    var existingItem = serviceCart.FirstOrDefault(d => d.ServiceID == detail.ServiceID);
                    if (existingItem != null)
                    {
                        existingItem.Quantity += quantity;
                    }
                    else
                    {
                        serviceCart.Add(detail);
                    }
                    Session["ServiceCart"] = serviceCart;
                }
            }
            else if (type == "product")
            {
                var product = db.Products.Find(itemId);
                if (product != null)
                {
                    var sale = new ProductSale
                    {
                        InvoiceID = invoiceId,
                        ProductID = itemId,
                        Quantity = quantity,
                        UnitPrice = product.UnitPrice
                    };
                    var existingItem = productCart.FirstOrDefault(p => p.ProductID == sale.ProductID);
                    if (existingItem != null)
                    {
                        existingItem.Quantity += quantity;
                    }
                    else
                    {
                        productCart.Add(sale);
                    }
                    Session["ProductCart"] = productCart;
                }
            }

            return RedirectToAction("CreateInvoice", new { id = invoiceId });
        }

        // POST: Invoices/ConfirmInvoice
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmInvoice(int invoiceId)
        {
            var serviceCart = Session["ServiceCart"] as List<InvoiceDetail> ?? new List<InvoiceDetail>();
            var productCart = Session["ProductCart"] as List<ProductSale> ?? new List<ProductSale>();
            var invoice = db.Invoices.Find(invoiceId);
            if (invoice != null && (serviceCart.Any() || productCart.Any()))
            {
                invoice.TotalAmount = serviceCart.Sum(d => d.UnitPrice * d.Quantity) + productCart.Sum(p => p.UnitPrice * p.Quantity);
                foreach (var detail in serviceCart)
                {
                    db.InvoiceDetails.Add(detail);
                }
                foreach (var sale in productCart)
                {
                    db.ProductSales.Add(sale);
                }
                db.SaveChanges();
                Session["ServiceCart"] = null;
                Session["ProductCart"] = null;
                return RedirectToAction("Index");
            }
            return RedirectToAction("CreateInvoice", new { id = invoiceId });
        }

        // GET: Invoices/Edit/5
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

        // POST: Invoices/Edit/5
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

        // GET: Invoices/Delete/5
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

        // POST: Invoices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Invoice invoice = db.Invoices.Find(id);
            db.Invoices.Remove(invoice);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Invoices/Payments/5
        public ActionResult Payments(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invoice invoice = db.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Payments)
                .FirstOrDefault(i => i.InvoiceID == id);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            return View(invoice);
        }

        // GET: Invoices/AddPayment/5
        public ActionResult AddPayment(int? id)
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
            ViewBag.Invoice = invoice;
            return View(new Payment { InvoiceID = id.Value, PaymentDate = DateTime.Now });
        }

        // POST: Invoices/AddPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPayment([Bind(Include = "PaymentID,InvoiceID,Amount,PaymentDate,Method,Status")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                db.Payments.Add(payment);
                db.SaveChanges();
                return RedirectToAction("Payments", new { id = payment.InvoiceID });
            }
            ViewBag.Invoice = db.Invoices.Find(payment.InvoiceID);
            return View(payment);
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