using Microsoft.EntityFrameworkCore;

namespace Gudang.Services
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            
        }

        public DbSet<Models.Master.User> Users { get; set; }
        public DbSet<Models.Master.MasterRole> MasterRoles { get; set; }
    }
}
