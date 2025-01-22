using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EminAutoPrime.Models;
using Microsoft.AspNetCore.Identity;

namespace EminAutoPrime.Data
{
    public class ApplicationDbContext : IdentityDbContext<AplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Kampanya> Kampanyalar { get; set; }
        public DbSet<AracMarkalari> AracMarkalari { get; set; }
        public DbSet<AracModelleri> AracModelleri { get; set; }
        public DbSet<Araclar> Araclar { get; set; }
        public DbSet<ServisAlanlari> ServisAlanlari { get; set; }
        public DbSet<ServisKayitlari> ServisKayitlari { get; set; }
        public DbSet<ServisDurumlari> ServisDurumlari { get; set; }
        public DbSet<ServisIslemleri> ServisIslemleri { get; set; }
        public DbSet<Parcalar> Parcalar { get; set; }
        public DbSet<ParcaKategorileri> ParcaKategorileri { get; set; }
        public DbSet<Randevular> Randevular { get; set; }
        public DbSet<RandevuDurumlari> RandevuDurumlari { get; set; }
        public DbSet<ServisIslemTipleri> ServisIslemTipleri { get; set; }
        public DbSet<KullaniciYorumlari> KullaniciYorumlari { get; set; }
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

            modelBuilder.Entity<ServisAlanlari>()
                .HasKey(sa => sa.ServisAlaniId);

           
            modelBuilder.Entity<ServisKayitlari>()
                .HasKey(sk => sk.ServisId);

            modelBuilder.Entity<ServisKayitlari>()
                .HasOne(s => s.Arac)
                .WithMany()
                .HasForeignKey(s => s.AracId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServisKayitlari>()
                .HasOne(sk => sk.Durum)
                .WithMany(d => d.ServisKayitlari)
                .HasForeignKey(sk => sk.DurumId);

           
            modelBuilder.Entity<ServisDurumlari>()
                .HasKey(sd => sd.DurumId);

            
            modelBuilder.Entity<ServisIslemleri>()
                .HasKey(si => si.IslemId);

            modelBuilder.Entity<ServisIslemleri>()
                .HasOne(si => si.Servis)
                .WithMany(sk => sk.ServisIslemleri)
                .HasForeignKey(si => si.ServisId);

            modelBuilder.Entity<ServisIslemleri>()
                .HasOne(si => si.ServisAlani)
                .WithMany(sa => sa.ServisIslemleri)
                .HasForeignKey(si => si.ServisAlaniId);

            modelBuilder.Entity<ServisIslemleri>()
                .HasOne(si => si.TakilanParca)
                .WithMany(p => p.ServisIslemleri)
                .HasForeignKey(si => si.TakilanParcaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ServisIslemleri>()
                .HasOne(s => s.Arac)
                .WithMany(a => a.ServisIslemleri)
                .HasForeignKey(s => s.AracId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServisIslemleri>()
                .HasOne(si => si.IslemTip)
                .WithMany(it => it.ServisIslemleri)
                .HasForeignKey(si => si.IslemTipId);

       
            modelBuilder.Entity<Parcalar>()
                .HasKey(p => p.ParcaId);

            modelBuilder.Entity<Parcalar>()
                .HasOne(p => p.Kategori)
                .WithMany(pk => pk.Parcalar)
                .HasForeignKey(p => p.KategoriId);

            modelBuilder.Entity<ParcaKategorileri>()
                .HasKey(pk => pk.KategoriId);


            modelBuilder.Entity<Randevular>()
                .HasOne(r => r.Arac)
                .WithMany()
                .HasForeignKey(r => r.AracId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Randevular>()
                .HasOne(r => r.Musteri)
                .WithMany()
                .HasForeignKey(r => r.MusteriId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Randevular>()
                .HasOne(r => r.ServisAlani)
                .WithMany(sa => sa.Randevular)
                .HasForeignKey(r => r.ServisAlaniId);

            modelBuilder.Entity<Randevular>()
                .HasOne(r => r.Arac)
                .WithMany()
                .HasForeignKey(r => r.AracId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Randevular>()
                .HasOne(r => r.Durum)
                .WithMany(rd => rd.Randevular)
                .HasForeignKey(r => r.DurumId);
                       
            modelBuilder.Entity<RandevuDurumlari>()
                .HasKey(rd => rd.DurumId);
            
            modelBuilder.Entity<ServisIslemTipleri>()
                .HasKey(sit => sit.IslemTipId);

            modelBuilder.Entity<KullaniciYorumlari>()
                .HasKey(y => y.YorumId);

            modelBuilder.Entity<KullaniciYorumlari>()
                .HasOne(y => y.Kullanici)
                .WithMany()
                .HasForeignKey(y => y.KullaniciId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
