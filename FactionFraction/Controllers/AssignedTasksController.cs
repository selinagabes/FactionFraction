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
                _context.Add(assignedTask);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
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
        static double SumGrades(int[] grades) {
            
            int total = 0;
            for (int i = 0; i < grades.Length; ++i) {
                total += grades[i];
            }

            return total;
        }


        // Determine average grade from all submitted
        static double AverageGrade(int[] grades) {
            return Utility.SumGrades(grades) / grades.Length;    
        }


        // Determine the percentage workload given a desired grade
        static double DetermineWorkPercentage(int[] grades, int grade) {
            return grade / Utility.SumGrades(grades);
        } 
    }
}
