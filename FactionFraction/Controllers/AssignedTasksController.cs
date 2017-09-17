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
        private double SumGrades(int[] grades) {
            
            int total = 0;
            for (int i = 0; i < grades.Length; ++i) {
                total += grades[i];
            }

            return total;
        }


        // Determine average grade from all submitted
        private double AverageGrade(int[] grades) {
            return SumGrades(grades) / grades.Length;    
        }


        // Determine the percentage workload given a desired grade
        private double DetermineWorkPercentage(int[] grades, int grade) {
            return grade / SumGrades(grades);
        } 



        // Calculate estimated completion time for each task 
        private void CalculateEstimatedTimeForTasks(List<AssignedTask> tasks, List<GroupMember> members) {

            List<int> taskTotal = new List<int>();
            List<int> timeTotal = new List<int>();

            for (int i = 0; i < tasks.Count; ++i) {
                foreach (var member in members) {
                    taskTotal.Add(member.ProposedMinutes.ToList()[i].Length);
                }

                // Set estimated completion time for this task
                // TODO: Save to entity
                tasks[i].EstimatedMinutes = (int) Math.Round(taskTotal.Average());
                taskTotal.Clear();
            }
        }


        // Determine the completion time for the whole project based already defined estimated times
        private double EstimatedProjectCompletionTime(List<AssignedTask> tasks) {
            double totalTime = 0;

            // Sum the average times for all tasks (should already be set!!)
            for (int i = 0; i < tasks.Count; ++i) {
                totalTime += tasks[i].EstimatedMinutes;
            }

            return totalTime;
        }


        // Map tasks to group members based exclusively on task completion estimations
        private void AssignTasks(List<AssignedTask> tasks, List<GroupMember> members) {


            // Sort the tasks ascending in estimated completion time
            tasks = tasks.OrderBy(task => task.EstimatedMinutes).ToList();

            List<GroupMember> candidates = members;

            // Find lowest completion time amongst all candidates for each task
            for (var i = 0; i < tasks.Count; ++i) {
                GroupMember bestMember = new GroupMember();

                // If all members have been assigned, "refill" candiates for more tasks!
                if (candidates.Count == 0) { 
                    candidates = members; 
                }

                foreach (var member in candidates) {

                    // If this is the shortest proposed completion time, update new shortest
                    if (member.ProposedMinutes.ToList()[i].Length < bestMember.ProposedMinutes.ToList()[i].Length) {
                        bestMember = member;
                    }
                }
                

                // Set this task's group member, add task to group member's task list
                // TODO: Save entity
                tasks[i].GroupMember = bestMember;
                bestMember.AssignedTasks.Add(tasks[i]);

                candidates.Remove(bestMember);
            }
        }
    }
}
