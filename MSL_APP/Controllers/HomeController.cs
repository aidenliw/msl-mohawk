﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MSL_APP.Data;
using MSL_APP.Models;

namespace MSL_APP.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public HomeController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> RoleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = RoleManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Student()
        {
            // get current logged in user id
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Join to tables together
            var query = (from pk in _context.ProductKey
                         join pn in _context.ProductName on pk.NameId equals pn.Id
                         join ac in _userManager.Users on pk.OwnerId equals ac.StudentId
                         where ac.Id == userId
                         select new StudentKey
                         {
                             Product = pn.Name,
                             Key = pk.Key,
                             DownloadLink = pn.DownloadLink
                         });

            return View(await query.ToListAsync());
        }

        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> Admin(string sortBy, string search)
        {
            var products = _context.ProductName.AsQueryable();

            // Search product by the input
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.Name.ToLower().Contains(search.ToLower()));
            }

            ViewBag.SortByProduct = string.IsNullOrEmpty(sortBy) ? "NameDESC" : "";
            ViewBag.SortByTotalKeys = sortBy == "TotalKey" ? "TotalKeyDESC" : "TotalKey";
            ViewBag.SortByAvailableKeys = sortBy == "AvailableKey" ? "AvailableKeyDESC" : "AvailableKey";
            ViewBag.SortByUsedKeys = sortBy == "UsedKey" ? "UsedKeyDESC" : "UsedKey";

            // Sort the product by name
            switch (sortBy) 
            {
                case "NameDESC":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                case "TotalKeyDESC":
                    products = products.OrderByDescending(p => p.KeyCount);
                    break;
                case "TotalKey":
                    products = products.OrderBy(p => p.KeyCount);
                    break;
                case "AvailableKeyDESC":
                    products = products.OrderByDescending(p => p.RemainingKeyCount);
                    break;
                case "AvailableKey":
                    products = products.OrderBy(p => p.RemainingKeyCount);
                    break;
                case "UsedKeyDESC":
                    products = products.OrderByDescending(p => p.UsedKeyCount);
                    break;
                case "UsedKey":
                    products = products.OrderBy(p => p.UsedKeyCount);
                    break;
                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }
            
            return View(await products.ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminSearchProduct()
        {

            return View(await _context.ProductName.ToListAsync());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
                Email = "student1@email.com",
                UserName = "student1@email.com",
                FirstName = "Student",
                LastName = "One",
                ActiveStatus = "Actived"
            });

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
                Email = "Admin@email.com",
                UserName = "Admin@email.com",
                FirstName = "Admin",
                LastName = "One",
                ActiveStatus = "Actived"
            });


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