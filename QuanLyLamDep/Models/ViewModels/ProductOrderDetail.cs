using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyLamDep.Models.ViewModels
{
    public class ProductOrderDetail
    {
        public int ProductOrderDetailID { get; set; }

        public int ProductOrderID { get; set; }
        public virtual ProductOrder ProductOrder { get; set; }

        public int ProductID { get; set; }
        public virtual Product Product { get; set; }

        public int Quantity { get; set; }
    }
}