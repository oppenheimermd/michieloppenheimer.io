using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable once CheckNamespace
namespace Blog.Web.Controllers
{
    public class BlogController : Controller
    {
        public BlogController()
        {
            
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}