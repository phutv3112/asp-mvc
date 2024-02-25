using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppMVC.Models;
using Microsoft.AspNetCore.Authorization;

namespace AppMVC.Areas.AdminCP.Controllers
{
    [Area("AdminCP")]
    [Authorize(Roles = "Admin")]
    public class AdminCPController : Controller
    {
        private readonly AppDbContext _context;

        public AdminCPController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Category
        [Route("/admincp")]
        public async Task<IActionResult> Index()
        {

            return View();
        }

    }
}
