using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyLamDep.Models.ViewModels
{
    public class ServiceViewModel
    {
        public int ServiceID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public string ImageUrl { get; set; }
    }

}