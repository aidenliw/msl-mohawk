﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MSL_APP.Data;
using MSL_APP.Models;

namespace MSL_APP.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> RoleManager)
        {
            _userManager = userManager;
            _roleManager = RoleManager;
        }

        //Seed the database with users, roles and assign users to roles. To call this method, use https://localhost:44350/Account/SeedUserData
        public async Task<IActionResult> SeedUserData()
        {
            //Variable to hold the status of our identity operations
            IdentityResult result;

            //Create 2 new roles (Student, Admin)
            result = await _roleManager.CreateAsync(new IdentityRole("Student"));
            if (!result.Succeeded)
                return View("Error", new ErrorViewModel { RequestId = "Failed to add Student role" });

            result = await _roleManager.CreateAsync(new IdentityRole("Admin"));
            if (!result.Succeeded)
                return View("Error", new ErrorViewModel { RequestId = "Failed to add Admin role" });


            //Create a list of students
            List<ApplicationUser> StudentList = new List<ApplicationUser>();

            //Sample bank clients
            StudentList.Add(new ApplicationUser
            {
                Email = "student1@email.com",
                UserName = "student1@email.com"
            });

            foreach (ApplicationUser student in StudentList)
            {
                //Create the new user with password "Mohawk1!"
                result = await _userManager.CreateAsync(student, "Mohawk1!");
                if (!result.Succeeded)
                    return View("Error", new ErrorViewModel { RequestId = "Failed to add new user" });
                //Assign the new user to the student role
                result = await _userManager.AddToRoleAsync(student, "Student");
                if (!result.Succeeded)
                    return View("Error", new ErrorViewModel { RequestId = "Failed to assign student role" });

            }

            //Create a list of admins
            List<ApplicationUser> AdminsList = new List<ApplicationUser>();

            //Sample bank admins
            AdminsList.Add(new ApplicationUser
            {
                Email = "Admin@email.com",
                UserName = "Admin@email.com"
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
            return Content("Users setup completed");
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}