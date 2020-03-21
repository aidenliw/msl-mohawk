﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MSL_APP.Data;
using MSL_APP.Models;
using MSL_APP.Utility;

namespace MSL_APP.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> RoleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = RoleManager;
            _context = context;
        }

        // GET: Account
        public async Task<IActionResult> Index(string sortBy, string search, string currentFilter, int? pageNumber, int? pageRow, string roleType)
        {
            int pageSize = pageRow ?? 10;
            ViewData["totalRow"] = pageRow;
            ViewData["CurrentSort"] = sortBy;
            ViewData["AccId"] = string.IsNullOrEmpty(sortBy) ? "IdDESC" : "";
            ViewData["AccEmail"] = sortBy == "Email" ? "EmailDESC" : "Email";
            ViewData["AccFirstName"] = sortBy == "FirstName" ? "FirstNameDESC" : "FirstName";
            ViewData["AccLastName"] = sortBy == "LastName" ? "LastNameDESC" : "LastName";
            ViewData["AccStatus"] = sortBy == "ActiveStatus" ? "ActiveStatusDESC" : "ActiveStatus";

            if (string.IsNullOrEmpty(roleType)) { ViewData["AccRole"] = "Student"; }
            else if (roleType == "Admin") { ViewData["AccRole"] = "Admin"; }
            else { ViewData["AccRole"] = "Student"; }


            if (search != null)
            {
                pageNumber = 1;
            }
            else
            {
                search = currentFilter;
            }
            ViewData["CurrentFilter"] = search;

            var users = _userManager.Users.AsQueryable();

            // Search product by the input
            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(u => u.Email.ToLower().Contains(search.ToLower()) 
                || u.StudentId.ToString().Contains(search)
                || u.FirstName.ToLower().Contains(search.ToLower())
                || u.LastName.ToLower().Contains(search.ToLower()));
            }

            // Sort the product by name
            switch (sortBy)
            {
                case "IdDESC":
                    users = users.OrderByDescending(u => u.StudentId);
                    break;
                case "EmailDESC":
                    users = users.OrderByDescending(u => u.Email);
                    break;
                case "Email":
                    users = users.OrderBy(u => u.Email);
                    break;
                case "FirstNameDESC":
                    users = users.OrderByDescending(u => u.FirstName);
                    break;
                case "FirstName":
                    users = users.OrderBy(u => u.FirstName);
                    break;
                case "LastNameDESC":
                    users = users.OrderByDescending(u => u.LastName);
                    break;
                case "LastName":
                    users = users.OrderBy(u => u.LastName);
                    break;
                case "ActiveStatusDESC":
                    users = users.OrderByDescending(u => u.ActiveStatus);
                    break;
                case "ActiveStatus":
                    users = users.OrderBy(u => u.ActiveStatus);
                    break;
                default:
                    users = users.OrderBy(u => u.StudentId);
                    break;
            }

            if (pageRow == -1)
            {
                pageSize = users.Count();
                ViewData["totalRow"] = pageSize;
            }

            // Display admin or student account by user request
            switch (roleType)
            {
                case "Student":
                    users = users.Where(u => u.Role == "Student");
                    break;
                case "Admin":
                    users = users.Where(u => u.Role == "Admin");
                    break;
                default:
                    users = users.Where(u => u.Role == "Student");
                    break;
            }

            var model = await PaginatedList<ApplicationUser>.CreateAsync(users.AsNoTracking(), pageNumber ?? 1, pageSize);

            return View(model);
        }

        // GET: Account/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Account/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AdminID,FirstName,LastName,AdminEmail,Password,ConfirmPassword")] Account account)
        {
            //Check if given Id is a duplicate
            if (_userManager.Users.Any(e => e.StudentId == account.AdminID))
            {
                ModelState.AddModelError("AdminID", "Account ID already exists");
            }

            //Check if given email is a duplicate
            if (_userManager.Users.Any(e => e.Email == account.AdminEmail))
            {
                ModelState.AddModelError("AdminEmail", "Email already exists");
            }

            if (ModelState.IsValid) {
                ApplicationUser user = new ApplicationUser
                {
                    UserName = account.AdminEmail,
                    Email = account.AdminEmail,
                    FirstName = account.FirstName,
                    LastName = account.LastName,
                    StudentId = account.AdminID,
                    ActiveStatus = "Active",
                    Role = "Admin"
                };

                IdentityResult result = await _userManager.CreateAsync(user, "Mohawk1!");

                if (result.Succeeded)
                {
                    //Assign the new user to the admin role
                    var addRole = await _userManager.AddToRoleAsync(user, "Admin");

                    //return RedirectToAction(nameof(Index));
                    return RedirectToAction(nameof(Index), new { roleType = "Admin" });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(account);
            }

            IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var modelState in allErrors)
            {
                ModelState.AddModelError(string.Empty, modelState.ErrorMessage);
            }
            return View("Create");
            
        }

        private IActionResult Page()
        {
            throw new NotImplementedException();
        }

        // GET: Account/Ban/5
        public async Task<IActionResult> Ban(string id, string sortBy, string currentFilter, int? pageNumber, int? pageRow, string roleType)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _userManager.FindByIdAsync(id);

            if (account == null)
            {
                return NotFound();
            }
            else if (account.ActiveStatus == "Actived")
            {
                // Change the active status of the account to disabled
                account.ActiveStatus = "Disabled";
                _context.Entry(account).Property("ActiveStatus").IsModified = true;
                _context.SaveChanges();
                return RedirectToAction(nameof(Index), new { sortBy, currentFilter, pageNumber, pageRow, roleType });
            }
            else if (account.ActiveStatus == "Disabled")
            {
                // Change the active status of the account to disabled
                account.ActiveStatus = "Actived";
                _context.Entry(account).Property("ActiveStatus").IsModified = true;
                _context.SaveChanges();
                return RedirectToAction(nameof(Index), new { sortBy, currentFilter, pageNumber, pageRow, roleType });
            }
            return View("Index");
        }

        // GET: Account/Delete/5
        public async Task<IActionResult> Delete(string id, string sortBy, string currentFilter, int? pageNumber, int? pageRow, string roleType)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _userManager.FindByIdAsync(id);

            if (account == null)
            {
                return NotFound();
            }
            else
            {
                var result = await _userManager.DeleteAsync(account);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index), new { sortBy, currentFilter, pageNumber, pageRow, roleType });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("Index");
            }
        }


        //Seed the database with users, roles and assign users to roles. To call this method, use https://localhost:44350/Account/SeedUserData
        public async Task<IActionResult> SeedUserData()
        {
            //Variable to hold the status of our identity operations
            IdentityResult result;

            //Create 2 new roles (Student, Admin)
            if (await _roleManager.RoleExistsAsync("Student") == false)
            {
                result = await _roleManager.CreateAsync(new IdentityRole("Student"));
                if (!result.Succeeded)
                    return View("Error", new ErrorViewModel { RequestId = "Failed to add Student role" });
            }

            if (await _roleManager.RoleExistsAsync("Admin") == false)
            {
                result = await _roleManager.CreateAsync(new IdentityRole("Admin"));
                if (!result.Succeeded)
                    return View("Error", new ErrorViewModel { RequestId = "Failed to add Admin role" });
            }

            //Create a list of students
            List<ApplicationUser> StudentList = new List<ApplicationUser>();

            //Sample student user
            StudentList.Add(new ApplicationUser
            {
                StudentId = 000777777,
                Email = "student1@email.com",
                UserName = "student1@email.com",
                FirstName = "Student",
                LastName = "One",
                ActiveStatus = "Actived",
                Eligible = "Yes",
                Role = "Student"
            });

            EligibleStudent studentAccount = new EligibleStudent()
            {
                StudentID = 000777777,
                StudentEmail = "student1@email.com",
                FirstName = "Student",
                LastName = "One",
            };
            _context.EligibleStudent.Add(studentAccount);

            foreach (ApplicationUser student in StudentList)
            {
                //Create the new user with password "Mohawk1!"
                result = await _userManager.CreateAsync(student, "Mohawk1!");
                if (!result.Succeeded)
                    return View("Error", new ErrorViewModel { RequestId = "Failed to add new student user" });
                //Assign the new user to the student role
                result = await _userManager.AddToRoleAsync(student, "Student");
                if (!result.Succeeded)
                    return View("Error", new ErrorViewModel { RequestId = "Failed to assign student role" });

            }

            //Create a list of admins
            List<ApplicationUser> AdminsList = new List<ApplicationUser>();

            //Sample admin user
            AdminsList.Add(new ApplicationUser
            {
                StudentId = 000101010,
                Email = "Admin@email.com",
                UserName = "Admin@email.com",
                FirstName = "Admin",
                LastName = "One",
                ActiveStatus = "Actived",
                Eligible = "Yes",
                Role = "Admin"
            });

            EligibleStudent adminAccount = new EligibleStudent()
            {
                StudentID = 000101010,
                StudentEmail = "Admin@email.com",
                FirstName = "Admin",
                LastName = "One",
            };
            _context.EligibleStudent.Add(adminAccount);

            foreach (ApplicationUser admin in AdminsList)
            {
                //Create the new user with password "Mohawk1!"
                result = await _userManager.CreateAsync(admin, "Mohawk1!");
                if (!result.Succeeded)
                    return View("Error", new ErrorViewModel { RequestId = "Failed to add new admin user" });
                //Assign the new user to the admin role
                result = await _userManager.AddToRoleAsync(admin, "Admin");
                if (!result.Succeeded)
                    return View("Error", new ErrorViewModel { RequestId = "Failed to assign admin role" });

            }



            //If we are here, everything executed according to plan, so we will show a success message
            return Content("Users setup completed.\n\n" +
                "Admin Account:\n" +
                "Username = Admin@email.com\n" +
                "Password = Mohawk1!\n\n" +
                "Student Account:\n" +
                "Username = student1@email.com\n" +
                "Password = Mohawk1!\n");
        }


    }
}