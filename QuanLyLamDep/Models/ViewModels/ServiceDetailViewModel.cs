using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyLamDep.Models.ViewModels
{
    public class ServiceDetailViewModel
    {
        public int ServiceID { get; set; }
        public string ServiceName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}