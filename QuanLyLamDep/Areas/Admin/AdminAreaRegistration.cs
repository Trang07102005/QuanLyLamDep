﻿using System.Web.Mvc;

namespace QuanLyLamDep.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration 
    {
        public override string AreaName
        {
            get
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { area = "Admin", action = "Index", id = UrlParameter.Optional },
                new[] { "QuanLyLamDep.Areas.Admin.Controllers" }
            );
        }
    }
}