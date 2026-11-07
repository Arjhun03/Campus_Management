using Campus_Management.Models;
using Campus_Management.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Campus_Management.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var db = serviceProvider.GetRequiredService<ApplicationDbContext>();

        // 1. Ensure all 4 primary roles (+ Admin alias) exist
        string[] roles = ["SuperAdmin", "Admin", "PlacementOfficer", "CompanyHR", "Student"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed SuperAdmin / Admin
        var adminEmail = serviceProvider.GetRequiredService<IConfiguration>()["SeedUsers:AdminEmail"];
        if (string.IsNullOrWhiteSpace(adminEmail)) adminEmail = "admin@gmail.com";
        var adminPassword = serviceProvider.GetRequiredService<IConfiguration>()["SeedUsers:AdminPassword"];
        if (string.IsNullOrWhiteSpace(adminPassword)) adminPassword = "Admin@123";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Super Administrator",
                EmailConfirmed = true,
                IsActive = true
            };
            var createRes = await userManager.CreateAsync(adminUser, adminPassword);
            if (createRes.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        else
        {
            if (!await userManager.IsInRoleAsync(adminUser, "SuperAdmin"))
                await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // 3. Seed Base College if needed
        var defaultCollege = await db.Colleges.FirstOrDefaultAsync();
        if (defaultCollege == null)
        {
            defaultCollege = new College
            {
                CollegeName = "National Institute of Technology",
                City = "Tiruchirappalli",
                Address = "Tanjore Main Road, National Highway 67",
                PhoneNumber = "+91 431 2503000",
                Email = "director@nitt.edu",
                Status = CollegeStatus.Active
            };
            db.Colleges.Add(defaultCollege);
            await db.SaveChangesAsync();
        }

        // 4. Seed Placement Officer
        const string officerEmail = "officer@nit.edu";
        const string officerPassword = "Officer@123";
        var officerUser = await userManager.FindByEmailAsync(officerEmail);
        if (officerUser == null)
        {
            officerUser = new ApplicationUser
            {
                UserName = officerEmail,
                Email = officerEmail,
                FullName = "Dr. Suresh Placement Head",
                EmailConfirmed = true,
                IsActive = true
            };
            var createRes = await userManager.CreateAsync(officerUser, officerPassword);
            if (createRes.Succeeded)
            {
                await userManager.AddToRoleAsync(officerUser, "PlacementOfficer");
            }
        }
        var officerRecord = await db.PlacementOfficers.FirstOrDefaultAsync(p => p.Email == officerEmail);
        if (officerRecord == null)
        {
            officerRecord = new PlacementOfficer
            {
                FullName = "Dr. Suresh Placement Head",
                Email = officerEmail,
                PhoneNumber = "+91 9444012345",
                CollegeId = defaultCollege.CollegeId
            };
            db.PlacementOfficers.Add(officerRecord);
            await db.SaveChangesAsync();
        }

        // 5. Seed Base Company if needed
        var defaultCompany = await db.Companies.FirstOrDefaultAsync();
        if (defaultCompany == null)
        {
            defaultCompany = new Company
            {
                CompanyName = "Tata Consultancy Services",
                CompanyEmail = "contact@tcs.com",
                Industry = "IT & Consulting",
                Website = "https://www.tcs.com",
                Address = "SIPCOT IT Park, Siruseri, Chennai",
                Description = "Global leader in IT services, consulting, and business solutions.",
                Status = CompanyStatus.Active,
                CreatedAt = DateTime.UtcNow
            };
            db.Companies.Add(defaultCompany);
            await db.SaveChangesAsync();
        }

        // 6. Seed Company HR
        const string hrEmail = "sarah.hr@tcs.com";
        const string hrPassword = "HrPassword@123";
        var hrUser = await userManager.FindByEmailAsync(hrEmail);
        if (hrUser == null)
        {
            hrUser = new ApplicationUser
            {
                UserName = hrEmail,
                Email = hrEmail,
                FullName = "Sarah Jenkins",
                EmailConfirmed = true,
                IsActive = true
            };
            var createRes = await userManager.CreateAsync(hrUser, hrPassword);
            if (createRes.Succeeded)
            {
                await userManager.AddToRoleAsync(hrUser, "CompanyHR");
            }
        }
        var hrRecord = await db.CompanyHRs.FirstOrDefaultAsync(h => h.UserId == hrUser.Id);
        if (hrRecord == null)
        {
            hrRecord = new CompanyHR
            {
                UserId = hrUser.Id,
                CompanyId = defaultCompany.CompanyId,
                Designation = "Lead Technical Recruiter",
                ApprovedBy = adminUser.Id,
                ApprovedAt = DateTime.UtcNow
            };
            db.CompanyHRs.Add(hrRecord);
            await db.SaveChangesAsync();
        }

        // 7. Seed Student
        const string studentEmail = "arun.cse@nit.edu";
        const string studentPassword = "Student@123";
        var studentUser = await userManager.FindByEmailAsync(studentEmail);
        if (studentUser == null)
        {
            studentUser = new ApplicationUser
            {
                UserName = studentEmail,
                Email = studentEmail,
                FullName = "Arun Kumar",
                EmailConfirmed = true,
                IsActive = true
            };
            var createRes = await userManager.CreateAsync(studentUser, studentPassword);
            if (createRes.Succeeded)
            {
                await userManager.AddToRoleAsync(studentUser, "Student");
            }
        }
        var studentRecord = await db.Students.FirstOrDefaultAsync(s => s.UserId == studentUser.Id);
        if (studentRecord == null)
        {
            studentRecord = new Student
            {
                UserId = studentUser.Id,
                CollegeId = defaultCollege.CollegeId,
                PlacementOfficerId = officerRecord.PlacementOfficerId,
                RegisterNumber = "NIT2026CSE001",
                Department = "Computer Science and Engineering",
                Degree = "B.Tech",
                GraduationYear = 2026,
                CGPA = 8.75m,
                ActiveBacklogs = 0,
                Status = StudentStatus.Approved,
                ApprovedAt = DateTime.UtcNow
            };
            db.Students.Add(studentRecord);
            await db.SaveChangesAsync();
        }
    }
}
