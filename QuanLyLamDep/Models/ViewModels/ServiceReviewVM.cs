using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyLamDep.Models.ViewModels
{
    public class ServiceReviewVM
    {
        public string UserName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}