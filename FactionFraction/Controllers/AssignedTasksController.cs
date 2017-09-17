using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FactionFraction.Data;
using FactionFraction.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FactionFraction.Controllers
{
    [Authorize]
    public class AssignedTasksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AssignedTasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AssignedTasks
        public async Task<IActionResult> Index()
        {
            return View(await _context.AssignedTasks.ToListAsync());
        }

        // GET: AssignedTasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignedTask = await _context.AssignedTasks
                .SingleOrDefaultAsync(m => m.Id == id);
            if (assignedTask == null)
            {
                return NotFound();
            }

            return View(assignedTask);
        }

        // GET: AssignedTasks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AssignedTasks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Completion,EstimatedMinutes")] AssignedTask assignedTask)
        {
            if (ModelState.IsValid)
            {
                var aspNetUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                assignedTask.AspNetUserId = aspNetUserId;
                _context.Add(assignedTask);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(assignedTask);
        }

        // GET: AssignedTasks/Estimate
        public IActionResult Estimate()
        {
            var aspNetUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var memberList = _context.GroupMembers.Where(x => x.AspNetUserId == aspNetUserId).ToList();
            var taskList = _context.AssignedTasks.Where(x => x.AspNetUserId == aspNetUserId).ToList();
            EstimateViewModel viewModel = new EstimateViewModel();

            viewModel.TaskTitles = taskList.Select(x => x.Title).ToList();
            viewModel.GroupMemberNames = memberList.Select(x => x.Name).ToList();
            foreach (var t in taskList)
            {
                foreach (var m in memberList)
                {
                    SuggestedMinute pm = new SuggestedMinute();
                    pm.TaskId = t.Id;
                    pm.GroupMemberId = m.Id;
                    viewModel.ProposedMinutes.Add(pm);
                }
            }


            return View(viewModel);
        }

        // POST: AssignedTasks/Estimate
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Estimate(EstimateViewModel assignedTask)
        {

            return View(assignedTask);
        }

        // GET: AssignedTasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignedTask = await _context.AssignedTasks.SingleOrDefaultAsync(m => m.Id == id);
            if (assignedTask == null)
            {
                return NotFound();
            }
            return View(assignedTask);
        }

        // POST: AssignedTasks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Completion,EstimatedMinutes")] AssignedTask assignedTask)
        {
            if (id != assignedTask.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(assignedTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssignedTaskExists(assignedTask.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(assignedTask);
        }

        // GET: AssignedTasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignedTask = await _context.AssignedTasks
                .SingleOrDefaultAsync(m => m.Id == id);
            if (assignedTask == null)
            {
                return NotFound();
            }

            return View(assignedTask);
        }

        // POST: AssignedTasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var assignedTask = await _context.AssignedTasks.SingleOrDefaultAsync(m => m.Id == id);
            _context.AssignedTasks.Remove(assignedTask);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AssignedTaskExists(int id)
        {
            return _context.AssignedTasks.Any(e => e.Id == id);
        }


        // MARK: - Utility functions
        // Determine sum of an array sum grades
        static double SumGrades(int[] grades)
        {

            int total = 0;
            for (int i = 0; i < grades.Length; ++i)
            {
                total += grades[i];
            }

            return total;
        }


        // Determine average grade from all submitted
        private double AverageGrade(int[] grades)
        {
            return SumGrades(grades) / grades.Length;
        }


        // Determine the percentage workload given a desired grade
        private double DetermineWorkPercentage(int[] grades, int grade)
        {
            return grade / SumGrades(grades);
        }
    }
}
