using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EminAutoPrime.Models;
using Microsoft.AspNetCore.Identity;

namespace EminAutoPrime.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Kampanya> Kampanyalar { get; set; }
        public DbSet<AracMarkalari> AracMarkalari { get; set; }
        public DbSet<AracModelleri> AracModelleri { get; set; }
        public DbSet<Araclar> Araclar { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<AracMarkalari>(entity =>
            {
                entity.HasKey(am => am.MarkaId); 

                entity.Property(am => am.MarkaAdi)
                      .IsRequired() 
                      .HasMaxLength(100);
            });

            
            modelBuilder.Entity<AracModelleri>(entity =>
            {
                entity.HasKey(am => am.ModelId); 

                entity.Property(am => am.ModelAdi)
                      .IsRequired() 
                      .HasMaxLength(100);

                entity.HasOne(am => am.Marka)
                      .WithMany(m => m.Modeller)
                      .HasForeignKey(am => am.MarkaId)
                      .OnDelete(DeleteBehavior.Restrict); 
            });

           
            modelBuilder.Entity<Araclar>(entity =>
            {
                entity.HasKey(a => a.AracId); 

                entity.Property(a => a.Plaka)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.HasOne(a => a.Marka)
                      .WithMany()
                      .HasForeignKey(a => a.MarkaId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Model)
                      .WithMany(m => m.Araclar)
                      .HasForeignKey(a => a.ModelId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Sahip)
                      .WithMany()
                      .HasForeignKey(a => a.SahipId)
                      .OnDelete(DeleteBehavior.Restrict); 
            });          

           
            modelBuilder.Entity<Kampanya>(entity =>
            {
                entity.HasKey(k => k.KampanyaID); 

                entity.Property(k => k.KampanyaBasligi)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(k => k.KampanyaAciklamasi)
                      .HasMaxLength(500);
            });
        }
    }
}
