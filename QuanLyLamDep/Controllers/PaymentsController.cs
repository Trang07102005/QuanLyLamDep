// PaymentsController.cs - Fully integrated with Invoice, Payment removed
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using QuanLyLamDep.Models;

namespace QuanLyLamDep.Controllers
{
    public class PaymentsController : Controller
    {
        private BeautySalonDBEntities db = new BeautySalonDBEntities();

        public ActionResult Index()
        {
            var invoices = db.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Employee)
                .ToList();
            return View(invoices);
        }

        public ActionResult QRCode(int invoiceId)
        {
            return View(model: invoiceId);
        }

        [HttpPost]
        public ActionResult HandlePaymentMethod(int InvoiceID, string Method)
        {
            if (Method == "Chuyển khoản")
            {
                return RedirectToAction("QRCode", new { invoiceId = InvoiceID });
            }
            if (Method == "Tiền mặt")
            {
                // Xử lý cập nhật trạng thái hóa đơn luôn và chuyển sang trang Success
                var invoice = db.Invoices.Find(InvoiceID);
                if (invoice == null || invoice.TotalAmount <= 0)
                {
                    TempData["Error"] = "❌ Hóa đơn không hợp lệ!";
                    return RedirectToAction("Index");
                }

                invoice.PaymentStatus = "Đã thanh toán";
                invoice.PaymentMethod = "Tiền mặt";
                db.Entry(invoice).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            // Nếu là Thẻ hoặc phương thức khác, vẫn qua ConfirmTransfer
            return RedirectToAction("ConfirmTransfer", new { invoiceId = InvoiceID, method = Method });
        }

        [HttpPost]
        public ActionResult ConfirmTransfer(int invoiceId, string method)
        {
            var invoice = db.Invoices.Find(invoiceId);
            if (invoice == null || invoice.TotalAmount <= 0)
            {
                TempData["Error"] = "❌ Hóa đơn không hợp lệ!";
                return RedirectToAction("Index");
            }

            var allowedMethods = new List<string> { "Tiền mặt", "Chuyển khoản", "Thẻ" };
            if (!allowedMethods.Contains(method))
            {
                TempData["Error"] = $"❌ Phương thức thanh toán không hợp lệ! (Method: {method})";
                return RedirectToAction("Index");
            }

            invoice.PaymentStatus = "Đã thanh toán";
            invoice.PaymentMethod = method;
            db.Entry(invoice).State = EntityState.Modified;
            db.SaveChanges();

            TempData["Success"] = "✅ Đã cập nhật hóa đơn!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult EnterCustomerInfo(string selectedServiceIds)
        {
            if (string.IsNullOrEmpty(selectedServiceIds))
                return RedirectToAction("Index", "Services");

            var serviceIds = selectedServiceIds.Split(',').Select(int.Parse).ToList();
            var services = db.Services.Where(s => serviceIds.Contains(s.ServiceID)).ToList();

            ViewBag.SelectedServiceIds = selectedServiceIds;
            ViewBag.Total = services.Sum(s => s.Price);
            return View(services);
        }
        [HttpGet]
        public ActionResult EnterCustomerInfoProduct(string selectedProductIds)
        {
            if (string.IsNullOrEmpty(selectedProductIds))
                return RedirectToAction("Index", "Products");

            var productIds = selectedProductIds.Split(',').Select(int.Parse).ToList();
            var products = db.Products.Where(p => productIds.Contains(p.ProductID)).ToList();

            var quantities = TempData["ProductQuantities"] as Dictionary<int, int>
                             ?? productIds.ToDictionary(id => id, id => 1); // fallback: quantity = 1

            ViewBag.ProductQuantities = quantities;
            ViewBag.SelectedProductIds = selectedProductIds;

            // ✅ Sửa chỗ này để tránh lỗi null
            ViewBag.Total = products.Sum(p =>
                quantities.ContainsKey(p.ProductID) ? p.UnitPrice * quantities[p.ProductID] : 0);

            return View("EnterCustomerInfoProduct", products);
        }


        [HttpPost]
        public ActionResult SubmitPayment(string FullName, string Phone, string Email, string selectedServiceIds)
        {
            if (string.IsNullOrEmpty(selectedServiceIds))
            {
                TempData["Error"] = "Vui lòng chọn dịch vụ trước khi thanh toán.";
                return RedirectToAction("Index", "Services");
            }

            var serviceIds = selectedServiceIds.Split(',').Select(int.Parse).ToList();

            var customer = new Customer
            {
                FullName = FullName,
                Phone = Phone,
                Email = Email,
                CreatedAt = DateTime.Now
            };
            db.Customers.Add(customer);
            db.SaveChanges();

            var invoice = new Invoice
            {
                CustomerID = customer.CustomerID,
                CreatedDate = DateTime.Now,
                TotalAmount = 0,
                PaymentMethod = "Tiền mặt",
                PaymentStatus = "Chưa thanh toán",
                EmployeeID = 1
            };
            db.Invoices.Add(invoice);
            db.SaveChanges();

            foreach (var id in serviceIds)
            {
                var service = db.Services.Find(id);
                db.InvoiceDetails.Add(new InvoiceDetail
                {
                    InvoiceID = invoice.InvoiceID,
                    ServiceID = id,
                    Quantity = 1,
                    UnitPrice = service.Price
                });
            }
            db.SaveChanges();

            invoice.TotalAmount = db.InvoiceDetails
                .Where(d => d.InvoiceID == invoice.InvoiceID)
                .Sum(d => d.Quantity * d.UnitPrice);
            db.Entry(invoice).State = EntityState.Modified;
            db.SaveChanges();

            TempData["Success"] = "Thanh toán thành công!";
            return RedirectToAction("Index", "Services");
        }

        [HttpPost]
        public ActionResult SubmitPaymentProduct(string FullName, string Phone, string Email, string selectedProductIds)
        {
            if (string.IsNullOrEmpty(selectedProductIds))
            {
                TempData["Error"] = "Vui lòng chọn sản phẩm trước khi thanh toán.";
                return RedirectToAction("Index", "Products");
            }

            var productIds = selectedProductIds.Split(',').Select(int.Parse).ToList();
            var quantities = TempData["ProductQuantities"] as Dictionary<int, int>
                             ?? productIds.ToDictionary(id => id, id => 1);

            var customer = new Customer
            {
                FullName = FullName,
                Phone = Phone,
                Email = Email,
                CreatedAt = DateTime.Now
            };
            db.Customers.Add(customer);
            db.SaveChanges();

            var invoice = new Invoice
            {
                CustomerID = customer.CustomerID,
                CreatedDate = DateTime.Now,
                TotalAmount = 0,
                PaymentMethod = "Tiền mặt",
                PaymentStatus = "Chưa thanh toán",
                EmployeeID = 1
            };
            db.Invoices.Add(invoice);
            db.SaveChanges();

            foreach (var id in productIds)
            {
                var product = db.Products.Find(id);
                var qty = quantities.ContainsKey(id) ? quantities[id] : 1;

                db.InvoiceDetails.Add(new InvoiceDetail
                {
                    InvoiceID = invoice.InvoiceID,
                    ProductID = id, // 👈 Sử dụng ProductID riêng thay vì ServiceID
                    Quantity = qty,
                    UnitPrice = product.UnitPrice
                });
            }
            db.SaveChanges();

            invoice.TotalAmount = db.InvoiceDetails
                .Where(d => d.InvoiceID == invoice.InvoiceID)
                .Sum(d => d.Quantity * d.UnitPrice);
            db.Entry(invoice).State = EntityState.Modified;
            db.SaveChanges();

            TempData["Success"] = "✅ Thanh toán sản phẩm thành công!";
            return RedirectToAction("Index", "Products");
        }

    }
}
