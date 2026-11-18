using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG6212PoE_ST10440987.Controllers;
using PROG6212PoE_ST10440987.Data;
using PROG6212PoE_ST10440987.Models;
using Claim = PROG6212PoE_ST10440987.Models.Claim;

namespace PROG6212.Testing
{
    public class LecturerTesting
    {
        [Fact]
        public async Task ViewLecturersClaims_Test()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_ForLecturerClaims")
                .Options;

            using var DbContext = new AppDbContext(options);

            var lecturer = new User { Id = Guid.NewGuid().ToString(), UserName = "Test_Lecturer" };
            DbContext.Users.Add(lecturer);

            DbContext.Claims.AddRange(
                new Claim { ClaimId = 1, claimStatus = "Pending", LecturerId = lecturer.Id, additionalNotes = "Work1", hoursWorked = 5, hourlyRate = 50, dateOfClaim = DateTime.Now },
                new Claim { ClaimId = 2, claimStatus = "Approved", LecturerId = lecturer.Id, additionalNotes = "Work2", hoursWorked = 3, hourlyRate = 50, dateOfClaim = DateTime.Now }
            );
            await DbContext.SaveChangesAsync();

            var controller = new LecturerController(null, DbContext, null, null);

            var userClaimsPrincipal = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(
                    new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "Test_Lecturer") },
                    "mock"
                )
            );

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };

            var result = await controller.ViewLecturerClaims() as ViewResult;

            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<List<Claim>>(result.Model);
            Assert.Equal(2, model.Count);
            Assert.All(model, c => Assert.Equal(lecturer.Id, c.LecturerId));
        }

        [Fact]
        public async Task SubmitClaim_Test()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "SubmitClaimTestDb")
                .Options;

            using var dbContext = new AppDbContext(options);

            var lecturer = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "TestLecturer"
            };
            dbContext.Users.Add(lecturer);
            await dbContext.SaveChangesAsync();

            var controller = new LecturerController(null, dbContext, null, null);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(ClaimTypes.Name, "TestLecturer")
            }, "mock"));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var claimModel = new Claim
            {
                additionalNotes = "Worked on project X",
                hoursWorked = 8
            };

            var result = await controller.SubmitClaim(claimModel);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Lecturer", redirectResult.ControllerName);

            var savedClaim = await dbContext.Claims.FirstOrDefaultAsync();
            Assert.NotNull(savedClaim);
            Assert.Equal("Pending", savedClaim.claimStatus);
            Assert.Equal(67, savedClaim.hourlyRate);
            Assert.Equal("TestLecturer", lecturer.UserName);
            Assert.Equal(claimModel.additionalNotes, savedClaim.additionalNotes);
            Assert.Equal(claimModel.hoursWorked, savedClaim.hoursWorked);
            Assert.Equal(lecturer.Id, savedClaim.LecturerId);
        }
    }
}
