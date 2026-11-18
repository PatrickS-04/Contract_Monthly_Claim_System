using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG6212PoE_ST10440987.Controllers;
using PROG6212PoE_ST10440987.Data;
using PROG6212PoE_ST10440987.Models;
using Claim = PROG6212PoE_ST10440987.Models.Claim;

namespace PROG6212.Testing
{
    public class ACAndPMTesting
    {
        [Fact]
        public async Task ApproveClaim_Test()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(databaseName: "TestDb_ForApprove").Options;

            using var DbContext = new AppDbContext(options);

            var claim = new Claim
            {
                ClaimId = 34,
                claimStatus = "Pending",
                hoursWorked = 10,
                hourlyRate = 100,
                additionalNotes = "Worked on project X",
                dateOfClaim = DateTime.Now
            };
            DbContext.Claims.Add(claim);
            await DbContext.SaveChangesAsync();

            var controller = new AcademicManagerController(DbContext);

            var result = await controller.ApproveClaim(34);

            var updatedClaim = await DbContext.Claims.FindAsync(34);
            Assert.Equal("Approved", updatedClaim.claimStatus);
        }

        [Fact]
        public async Task RejectClaim_Test()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(databaseName: "TestDb_ForReject").Options;

            using var DbContext = new AppDbContext(options);

            var claim = new Claim
            {
                ClaimId = 75,
                claimStatus = "Pending",
                hoursWorked = 12,
                hourlyRate = 100,
                additionalNotes = "Worked on project Y",
                dateOfClaim = DateTime.Now
            };
            DbContext.Claims.Add(claim);
            await DbContext.SaveChangesAsync();

            var controller = new AcademicManagerController(DbContext);

            var result = await controller.RejectClaim(75);

            var updatedClaim = await DbContext.Claims.FindAsync(75);
            Assert.Equal("Rejected", updatedClaim.claimStatus);
        }

        [Fact]
        public void ViewVerifiedClaims_Test()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(databaseName: "TestDb_ForReject").Options;

            using var DbContext = new AppDbContext(options);

            var lecturer = new User { Id = Guid.NewGuid().ToString(), UserName = "Test_Lecturer" };
            DbContext.Users.Add(lecturer);

            DbContext.Claims.AddRange(
                new Claim { ClaimId = 1, claimStatus = "pending", LecturerId = Guid.NewGuid().ToString(), additionalNotes = "Work1", hoursWorked = 5, hourlyRate = 50, dateOfClaim = DateTime.Now },
                new Claim { ClaimId = 2, claimStatus = "approved", LecturerId = Guid.NewGuid().ToString(), additionalNotes = "Work2", hoursWorked = 3, hourlyRate = 50, dateOfClaim = DateTime.Now },
                new Claim { ClaimId = 3, claimStatus = "pending", LecturerId = Guid.NewGuid().ToString(), additionalNotes = "Work3", hoursWorked = 8, hourlyRate = 50, dateOfClaim = DateTime.Now }
            );
            DbContext.SaveChanges();

            var controller = new AcademicManagerController(DbContext);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new System.Security.Claims.Claim[]
            {
                new System.Security.Claims.Claim(ClaimTypes.Name, "Test_Lecturer")
            }, "mock"));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var result = controller.ViewVerifiedClaims() as ViewResult;

            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<System.Collections.Generic.List<Claim>>(result.Model);

            Assert.Equal(2, model.Count);
            Assert.All(model, c => Assert.Equal("pending", c.claimStatus));
        }
    }
}
