using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EminAutoPrime.Models;

namespace EminAutoPrime.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<EminAutoArac> EminAutoAraclar { get; set; }
        public DbSet<EminAutoServis> EminAutoServisler { get; set; }
        public DbSet<Kampanya> Kampanyalar { get; set; }
    }
}
