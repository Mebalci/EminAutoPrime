using Microsoft.AspNetCore.Mvc;
using EminAutoPrime.Data;
using EminAutoPrime.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace EminAutoPrime.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> MusteriYonetimi(string searchTerm, int page = 1)
        {
            int pageSize = 10; // Her sayfada gösterilecek kullanıcı sayısı
            var users = _userManager.Users.ToList();

            // Arama
            if (!string.IsNullOrEmpty(searchTerm))
            {
                users = users.Where(u => u.UserName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }
                        
            int totalUsers = users.Count();
            int totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            var paginatedUsers = users.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                       
            var viewModel = new List<MusterilerViewModel>();
            foreach (var user in paginatedUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                viewModel.Add(new MusterilerViewModel
                {
                    UserId = user.Id,
                    Name = user.UserName,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }
            
            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;
            ViewData["SearchTerm"] = searchTerm;

            return View(viewModel);
        }

        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null && await _roleManager.RoleExistsAsync(role))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                               
                await _userManager.AddToRoleAsync(user, role);

                TempData["SuccessMessage"] = "Rol başarıyla atandı.";
            }
            else
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı veya rol geçersiz.";
            }

            return RedirectToAction("MusteriYonetimi");
        }


        public IActionResult Index()
        {
            var viewModel = new EminAutoAdminViewModel
            {
                TotalCars = _context.EminAutoAraclar.Count(),
                TotalServices = _context.EminAutoServisler.Count(),
                ToplamKampanyalar = _context.Kampanyalar.Count(),
                EminAutoAraclar = _context.EminAutoAraclar.Take(5).ToList(),
                EminAutoServisler = _context.EminAutoServisler.Take(5).ToList(),
                Kampanyalar = _context.Kampanyalar.Take(5).ToList()
            };

            return View(viewModel);
        }
    }
}
