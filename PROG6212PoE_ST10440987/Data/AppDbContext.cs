using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PROG6212PoE_ST10440987.Models;

namespace PROG6212PoE_ST10440987.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public AppDbContext()
        { 
        }

        public virtual DbSet<Claim> Claims { get; set; }
        public DbSet<SupportingDocument> SupportingDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Claim>()
                .HasOne(claim => claim.SupportingDocument)
                .WithOne(document => document.Claim)
                .HasForeignKey<SupportingDocument>(document => document.ClaimId);
        }
    }
}
