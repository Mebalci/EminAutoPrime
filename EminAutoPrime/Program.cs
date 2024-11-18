using EminAutoPrime.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EminAutoPrime
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Veri taban� ba�lant� dizesini al ve ApplicationDbContext'i yap�land�r
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Identity yap�land�rmas�
            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false; // E-posta onay� gerekmedi�i i�in false yap�yoruz
            })
            .AddRoles<IdentityRole>()  // Rolleri eklemek i�in bu sat�r� ekleyin
            .AddEntityFrameworkStores<ApplicationDbContext>();

            // Geli�tirici sayfa hata filtresi
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // MVC yap�land�rmas�
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Rolleri ve admin kullan�c�y� olu�tur
            await CreateRolesAndAdminUserAsync(app);

            // HTTP iste�i yap�land�rmas�
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
                    var result = await userManager.CreateAsync(adminUser, "Admin@123");  // G��l� bir �ifre belirleyin

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                    else
                    {
                        // Hatalar� loglay�n veya detayl� bilgi al�n
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
