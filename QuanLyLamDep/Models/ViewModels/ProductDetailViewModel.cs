using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyLamDep.Models.ViewModels
{
    public class ProductDetailViewModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}