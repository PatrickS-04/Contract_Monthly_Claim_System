using Microsoft.AspNetCore.Identity;
using PROG6212PoE_ST10440987.Models;

namespace PROG6212PoE_ST10440987.Data
{
    public static class InitializeDatabase
    {
        public static async Task TestUsers(IServiceProvider serviceProvider)
        {
            var managerOfRoles = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var managerOfUsers = serviceProvider.GetRequiredService<UserManager<User>>();

            string[] roleNames = new[] { "Lecturer", "Academic Manager", "Programme Coordinator", "HR" };
            foreach (var roleName in roleNames)
            {
                if (!await managerOfRoles.RoleExistsAsync(roleName))
                {
                    await managerOfRoles.CreateAsync(new IdentityRole(roleName));
                }
            }

            if (await managerOfUsers.FindByNameAsync("Lecturer_50") == null)
            {
                var user = new User { UserName = "Lecturer_50", Email = "Lecturer50@ymail.com", Name = "Benjamin", 
                    Surname = "Graham", HourlyRate = 34, ChangePassword = true };
                await managerOfUsers.CreateAsync(user, "Lecturer50");
                await managerOfUsers.AddToRoleAsync(user, "Lecturer");
            }

            if (await managerOfUsers.FindByNameAsync("AcademicManager_24") == null)
            {
                var user = new User { UserName = "AcademicManager_24", Email = "Manager24@ymail.com", Name = "Dean", 
                    Surname = "Shore", HourlyRate = 73, ChangePassword = true };
                await managerOfUsers.CreateAsync(user, "AcademicManager24");
                await managerOfUsers.AddToRoleAsync(user, "Academic Manager");
            }

            if (await managerOfUsers.FindByNameAsync("ProgrammeCoordinator_78") == null)
            {
                var user = new User { UserName = "ProgrammeCoordinator_78", Email = "Coordinator78@ymail.com", Name = "Pieter", 
                    Surname = "Schalk", HourlyRate = 65, ChangePassword = true };
                await managerOfUsers.CreateAsync(user, "ProgrammeCoordinator78");
                await managerOfUsers.AddToRoleAsync(user, "Programme Coordinator");
            }
            
            if (await managerOfUsers.FindByNameAsync("HR_352") == null)
            {
                var user = new User { UserName = "HR_352", Email = "HR352@ymail.com", Name = "Helen", 
                    Surname = "Fay", HourlyRate = 94, ChangePassword = true };
                await managerOfUsers.CreateAsync(user, "HelenFayHR352");
                await managerOfUsers.AddToRoleAsync(user, "HR");
            }
        }
    }
}
