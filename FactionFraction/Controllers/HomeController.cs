using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FactionFraction.Models;
using Microsoft.AspNetCore.Authorization;
using FactionFraction.Data;
using System.Security.Claims;

namespace FactionFraction.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var aspNetUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var groupMembers = _context.GroupMembers.Where(x => x.AspNetUserId == aspNetUserId).ToList();
            return View(groupMembers);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
