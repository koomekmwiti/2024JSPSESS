using ESS.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ESS.Data
{
    public class ApplicationDBContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {

        }

		public DbSet<DeftAddLeave> DeftAddLeave { get; set; }
        public DbSet<DeftAddAppraisal> DeftAddAppraisal { get; set; }

    }
}
