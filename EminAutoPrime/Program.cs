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

            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 10485760;
            });

            var app = builder.Build();

            // Rolleri ve admin kullanıcıyı oluştur
            await CreateRolesAndAdminUserAsync(app);

            // **Ek olarak, Rolleri Seed Etmek için RoleInitializer ekliyoruz**
            var scope = app.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await RoleInitializer.SeedRoles(roleManager);

            // HTTP isteği yapılandırması
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
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

        private static async Task CreateRolesAndAdminUserAsync(WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                string[] roles = { "Admin", "ServisCalisani", "Musteri" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                var adminUser = await userManager.FindByEmailAsync("admin@eminauto.com");
                if (adminUser == null)
                {
                    adminUser = new IdentityUser { UserName = "admin@eminauto.com", Email = "admin@eminauto.com" };
                    var result = await userManager.CreateAsync(adminUser, "Admin@123");  // Güçlü bir şifre belirleyin

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                    else
                    {
                        // Hataları loglayın veya detaylı bilgi alın
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine(error.Description);
                        }
                    }
                }
            }
        }
    }
}
