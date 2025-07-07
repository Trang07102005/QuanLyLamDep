using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyLamDep.Models.ViewModels
{
    public class InvoiceViewModel
    {
        public int InvoiceID { get; set; }
        public string CustomerName { get; set; }
        public string EmployeeName { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }

        public List<ServiceDetailViewModel> ServiceDetails { get; set; } = new List<ServiceDetailViewModel>();
        public List<ProductDetailViewModel> ProductDetails { get; set; } = new List<ProductDetailViewModel>();
        public List<PaymentViewModel> Payments { get; set; } = new List<PaymentViewModel>();
    }
}