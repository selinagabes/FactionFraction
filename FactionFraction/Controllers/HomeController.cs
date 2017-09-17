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

        public IActionResult Grade()
        {
            
            return View();
        }
        [HttpPost]
        public IActionResult Grade(float grade)
        {
            
            var aspNetUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var members = _context.GroupMembers.Where(x => x.AspNetUserId == aspNetUserId).ToList();
            var totalGrade = members.Select(x => x.DesiredGrade).Sum();
            Dictionary<int, float> UserId_MinCapacity = new Dictionary<int, float>();
            foreach (var m in members)
            {
                float pctWork = m.DesiredGrade / totalGrade;
            
                UserId_MinCapacity.Add(m.Id, pctWork);
            }
            var numOfMembers = members.Count();
            var gradeTotalValue = numOfMembers * grade;
            foreach(var user in UserId_MinCapacity)
            {
                var currentUser = _context.GroupMembers.Find(user.Key);
                currentUser.FinalGrade = user.Value * gradeTotalValue;
                _context.Update(currentUser);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Summary));
        }

        public IActionResult Summary()
        {
            Dictionary<string, float> finalGradeDictionary = new Dictionary<string, float>();
            var aspNetUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var members = _context.GroupMembers.Where(x => x.AspNetUserId == aspNetUserId).ToList();
            foreach(var m in members)
            {
                finalGradeDictionary.Add(m.Name, m.FinalGrade);
            }
            return View(finalGradeDictionary);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

