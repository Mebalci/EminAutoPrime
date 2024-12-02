using EminAutoPrime.Data;
using EminAutoPrime.Utilities;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EminAutoPrime
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Veri tabanı bağlantı dizesini al ve ApplicationDbContext'i yapılandır
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Identity yapılandırması
            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false; // E-posta onayı gerekmediği için false yapıyoruz
            })
            .AddRoles<IdentityRole>()  // Rolleri eklemek için bu satırı ekleyin
            .AddEntityFrameworkStores<ApplicationDbContext>();

            // Geliştirici sayfa hata filtresi
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // MVC yapılandırması
            builder.Services.AddControllersWithViews();

            // Form verileri için maksimum boyut sınırı (örneğin resim yükleme)
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 10485760; // 10 MB
            });

            var app = builder.Build();

            // Rolleri ve admin kullanıcıyı oluştur
            await SeedRolesAndAdminUserAsync(app);

            // HTTP isteği yapılandırması
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
                app.UseDeveloperExceptionPage(); // Hata detayları için
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }

        /// <summary>
        /// Rolleri ve admin kullanıcısını seed etmek için bu metot kullanılır.
        /// </summary>
        private static async Task SeedRolesAndAdminUserAsync(WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                // Rolleri oluştur
                string[] roles = { "Admin", "ServisCalisani", "Musteri" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // Admin kullanıcı oluştur
                var adminEmail = "admin@eminauto.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new IdentityUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail
                    };

                    var createAdminResult = await userManager.CreateAsync(adminUser, "Admin@123"); // Güçlü bir şifre kullanın
                    if (createAdminResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                    else
                    {
                        foreach (var error in createAdminResult.Errors)
                        {
                            Console.WriteLine(error.Description);
                        }
                    }
                }
            }
        }
    }
}
