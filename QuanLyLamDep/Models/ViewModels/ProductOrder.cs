using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyLamDep.Models.ViewModels
{
    public class ProductOrder
    {
        public int ProductOrderID { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public string CustomerName { get; set; }
        public string Phone { get; set; }

        public virtual ICollection<ProductOrderDetail> ProductOrderDetails { get; set; }
    }
}