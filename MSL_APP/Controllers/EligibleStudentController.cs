﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MSL_APP.Data;
using MSL_APP.Models;
using MSL_APP.Utility;

namespace MSL_APP.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EligibleStudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EligibleStudentController(ApplicationDbContext context)
        {
            _context = context; 
        }

        // GET: EligibleStudent
        public async Task<IActionResult> Index(string sortBy, string search, string currentFilter, int? pageNumber, int? pageRow)
        {
            int pageSize = pageRow ?? 10;
            ViewData["totalRow"] = pageRow;
            ViewData["CurrentSort"] = sortBy;
            ViewData["StudentId"] = string.IsNullOrEmpty(sortBy) ? "IdDESC" : "";
            ViewData["StudentEmail"] = sortBy == "Email" ? "EmailDESC" : "Email";
            ViewData["StudentFirstName"] = sortBy == "FirstName" ? "FirstNameDESC" : "FirstName";
            ViewData["StudentLastName"] = sortBy == "LastName" ? "LastNameDESC" : "LastName";
            
            if (search != null)
            {
                pageNumber = 1;
            }
            else
            {
                search = currentFilter;
            }
            ViewData["CurrentFilter"] = search;

            var students = _context.EligibleStudent.AsQueryable();
            // Search product by the input
            if (!string.IsNullOrEmpty(search))
            {
                students = students.Where(p => p.StudentEmail.ToLower().Contains(search.ToLower())
                || p.StudentID.ToString().Contains(search.TrimStart('0'))
                || p.FirstName.ToLower().Contains(search.ToLower())
                || p.LastName.ToLower().Contains(search.ToLower()));
            }

            // Sort the product by name
            switch (sortBy)
            {
                case "IdDESC":
                    students = students.OrderByDescending(s => s.StudentID);
                    break;
                case "EmailDESC":
                    students = students.OrderByDescending(s => s.StudentEmail);
                    break;
                case "Email":
                    students = students.OrderBy(s => s.StudentEmail);
                    break;
                case "FirstNameDESC":
                    students = students.OrderByDescending(s => s.FirstName);
                    break;
                case "FirstName":
                    students = students.OrderBy(s => s.FirstName);
                    break;
                case "LastNameDESC":
                    students = students.OrderByDescending(s => s.LastName);
                    break;
                case "LastName":
                    students = students.OrderBy(s => s.LastName);
                    break;
                default:
                    students = students.OrderBy(s => s.StudentID);
                    break;
            }

            if (pageRow == -1)
            {
                pageSize = students.Count();
                ViewData["totalRow"] = pageSize;
            }

            var model = await PaginatedList<EligibleStudent>.CreateAsync(students.AsNoTracking(), pageNumber ?? 1, pageSize);

            return View(model);
        }

        // GET: EligibleStudent/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eligibleStudent = await _context.EligibleStudent
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eligibleStudent == null)
            {
                return NotFound();
            }

            return View(eligibleStudent);
        }

        // GET: EligibleStudent/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EligibleStudent/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StudentID,FirstName,LastName,StudentEmail")] EligibleStudent eligibleStudent)
        {
            //Check if given Id is a duplicate
            if (_context.EligibleStudent.Any(e => e.StudentID == eligibleStudent.StudentID))
            {
                ModelState.AddModelError("StudentID", "Student ID already exists");
            }

            //Check if given email is a duplicate
            if (_context.EligibleStudent.Any(e => e.StudentEmail == eligibleStudent.StudentEmail))
            {
                ModelState.AddModelError("StudentEmail", "Student email already exists");
            }


            if (ModelState.IsValid)
            {
                _context.Add(eligibleStudent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(eligibleStudent);
        }

        // GET: EligibleStudent/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eligibleStudent = await _context.EligibleStudent.FindAsync(id);
            if (eligibleStudent == null)
            {
                return NotFound();
            }
            return View(eligibleStudent);
        }

        // POST: EligibleStudent/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StudentID,FirstName,LastName,StudentEmail")] EligibleStudent eligibleStudent)
        {
            if (id != eligibleStudent.Id)
            {
                return NotFound();
            }

            //Check if given Id is a duplicate
            if (_context.EligibleStudent.Any(e => e.StudentID == eligibleStudent.StudentID && e.Id != id))
            {
                ModelState.AddModelError("StudentID", "Student ID already exists");
            }

            //Check if given email is a duplicate
            if (_context.EligibleStudent.Any(e => e.StudentEmail == eligibleStudent.StudentEmail && e.Id != id))
            {
                ModelState.AddModelError("StudentEmail", "Student email already exists");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eligibleStudent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EligibleStudentExists(eligibleStudent.Id))
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
            return View(eligibleStudent);
        }

        // GET: EligibleStudent/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eligibleStudent = await _context.EligibleStudent
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eligibleStudent == null)
            {
                return NotFound();
            }

            return View(eligibleStudent);
        }

        // POST: EligibleStudent/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eligibleStudent = await _context.EligibleStudent.FindAsync(id);
            _context.EligibleStudent.Remove(eligibleStudent);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EligibleStudentExists(int id)
        {
            return _context.EligibleStudent.Any(e => e.Id == id);
        }

        /// <summary>
        /// Parses students from file, then creates entry for each.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return RedirectToAction("Index");

            try
            {
                var parser = new LicenseParser(file, ';');
                var results = parser.ParseStudents();

                //TODO: This is probably not performant; better to get a stored procedure to dump the table
                _context.EligibleStudent.RemoveRange(_context.EligibleStudent.ToList());

                foreach (EligibleStudent es in results.ValidList.Values)
                    _context.EligibleStudent.Add(es);
                
                await _context.SaveChangesAsync();
                TempData["InvalidList"] = results.InvalidList;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e}");
            }

            return RedirectToAction("Index");
        }
    }
}
