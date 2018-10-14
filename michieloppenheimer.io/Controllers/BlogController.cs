using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace michieloppenheimer.io.Controllers
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