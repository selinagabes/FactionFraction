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
            var aspNetUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var assignedTasks = _context.AssignedTasks;
            assignedTasks.Load();
            var listTasks = await assignedTasks.Where(x => x.AspNetUserId == aspNetUserId).ToListAsync();
            return View(listTasks);
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
        public IActionResult Estimate(EstimateViewModel assignedTask)
        {
            CalculateEstimatedTimeForTasks(assignedTask.ProposedMinutes);
            AssignTasks();
            return RedirectToAction(nameof(TaskSummary));
        }

        // GET: Task Summary
        public IActionResult TaskSummary()
        {
            var aspNetUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var taskList = _context.AssignedTasks.Include(m => m.GroupMember).Where(x => x.AspNetUserId == aspNetUserId).ToList();

            // set up view
            return View(taskList);
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
        // Determine average grade from all submitted
       
        // Calculate estimated completion time for each task 
        private void CalculateEstimatedTimeForTasks(List<SuggestedMinute> suggested)
        {
            var aspNetUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<AssignedTask> tasks = _context.AssignedTasks.Where(x => x.AspNetUserId == aspNetUserId).ToList();
            List<GroupMember> members = _context.GroupMembers.Where(x => x.AspNetUserId == aspNetUserId).ToList();

            List<int> taskTotal = new List<int>();
            List<int> timeTotal = new List<int>();

            for (int i = 0; i < tasks.Count; ++i)
            {
                foreach (var member in members)
                {
                    var currentTaskList = suggested.Where(x => x.TaskId == tasks[i].Id
                                                            && x.GroupMemberId == member.Id).Select(x => x.Length).ToList();
                    taskTotal.Add(currentTaskList.Sum());
                }

                // Set estimated completion time for this task              
                var assignedTask = _context.AssignedTasks.SingleOrDefault(m => m.Id == tasks[i].Id);
                assignedTask.EstimatedMinutes = (int)Math.Round(taskTotal.Average());
                _context.Update(assignedTask);
                _context.SaveChanges();
                taskTotal.Clear();
            }
        }


        // Determine the completion time for the whole project based already defined estimated times
        private float EstimatedProjectCompletionTime()
        {
            var aspNetUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<AssignedTask> tasks = _context.AssignedTasks.Where(x => x.AspNetUserId == aspNetUserId).ToList();

            float totalTime = 0;

            // Sum the average times for all tasks (should already be set!!)
            for (int i = 0; i < tasks.Count; ++i)
            {
                totalTime += tasks[i].EstimatedMinutes;
            }

            return totalTime;
        }


        // Map tasks to group members based exclusively on task completion estimations
        private void AssignTasks()
        {
            var aspNetUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<AssignedTask> tasks = _context.AssignedTasks.Where(x => x.AspNetUserId == aspNetUserId).ToList();
            List<GroupMember> members = _context.GroupMembers.Where(x => x.AspNetUserId == aspNetUserId).ToList();
            
            //Calculate percentage weight
            //Assign a capacity
            var totalGrade = members.Select(x => x.DesiredGrade).Sum();
            Dictionary<int, float> UserId_MinCapacity = new Dictionary<int, float>();
            var projectCompletionTime = EstimatedProjectCompletionTime();
            foreach (var m in members)
            {
                float pctWork = m.DesiredGrade / totalGrade;
                float capacity = pctWork * projectCompletionTime;
                UserId_MinCapacity.Add(m.Id, capacity);
            }


            // Sort the tasks ascending in estimated completion time
            tasks = tasks.OrderBy(task => task.EstimatedMinutes).ToList();
            var kvp_UserId_MinCapacity = UserId_MinCapacity.OrderBy(x => x.Value).ToList();
            var tasksCopy = tasks.ToList();
            // brute force, just give tasks to alternating people:
            var i = 0;
            while (tasksCopy.Any())
            {
                i = i % kvp_UserId_MinCapacity.Count;
                var taskToAssign = tasksCopy.First();
                var userMin = kvp_UserId_MinCapacity[i++];
                var currentMember = _context.GroupMembers.Find(userMin.Key);
                currentMember.AssignedTasks.Add(taskToAssign);
                _context.Update(currentMember);
                _context.SaveChanges();
                // remove task from list to be assigned.
                tasksCopy.Remove(taskToAssign);

            }


            //while (tasksCopy.Any())
            //{
            //    // assigning one task to a user.
            //    var taskToAssign = tasksCopy.First();
            //    // check to see if user has run out of capacity: 
            //    var userMin = kvp_UserId_MinCapacity.First();
            //    if (userMin.Value < -10 && kvp_UserId_MinCapacity.Count > 1)
            //    {
            //        // need to move to next user.
            //        kvp_UserId_MinCapacity.Remove(userMin);
            //    }
            //    else
            //    {
            //        // assign task to current user.
            //        var currentMember = _context.GroupMembers.Find(userMin.Key);
            //        currentMember.AssignedTasks.Add(taskToAssign);
            //        _context.Update(currentMember);
            //        _context.SaveChanges();
            //        // remove task from list to be assigned.
            //        tasksCopy.Remove(taskToAssign);
            //    }

            //}
            //int j = 0;
            //for (int i = 0; i < kvp_UserId_MinCapacity.Count(); i++)
            //{
            //    var capacity = kvp_UserId_MinCapacity[i].Value;
            //    while (capacity > -20)//threshold for overages
            //    {
            //        while (j < tasks.Count && tasks[j].GroupMember != null)
            //            j++;

            //        capacity -= tasks[j].EstimatedMinutes;
            //        if (capacity <= -20 && !tasks.Any(x => x.GroupMember == null))
            //            break;
            //        var currentMember = _context.GroupMembers.Find(kvp_UserId_MinCapacity[i].Key);
            //        currentMember.AssignedTasks.Add(tasks[j]);
            //        _context.Update(currentMember);
            //        _context.SaveChanges();
            //        j++;
            //    }
            //}
            //List<GroupMember> candidates = members;

            // Find lowest completion time amongst all candidates for each task
            //for (var i = 0; i < tasks.Count; ++i)
            //{
            //    GroupMember bestMember = new GroupMember();

            //    // If all members have been assigned, "refill" candiates for more tasks!
            //    if (candidates.Count == 0)
            //    {
            //        candidates = members;
            //    }

            //    foreach (var member in candidates)
            //    {

            //        // If this is the shortest proposed completion time, update new shortest
            //        if (member.ProposedMinutes.ToList()[i].Length < bestMember.ProposedMinutes.ToList()[i].Length)
            //        {
            //            bestMember = member;
            //        }
            //    }


            //    // Set this task's group member, add task to group member's task list
            //    // TODO: Save entity
            //    tasks[i].GroupMember = bestMember;
            //    bestMember.AssignedTasks.Add(tasks[i]);

            //    candidates.Remove(bestMember);
            //}
        }
    }
}
