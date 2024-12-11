using Microsoft.AspNetCore.Mvc;
using EminAutoPrime.Data;
using EminAutoPrime.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
            int pageSize = 5;
            var users = _userManager.Users.ToList();

            
            if (!string.IsNullOrEmpty(searchTerm))
            {
                users = users.Where(u => u.UserName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            int totalUsers = users.Count;
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
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles.ToList()
                });
            }

            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;
            ViewData["SearchTerm"] = searchTerm;

            return View(viewModel);
        }



        [HttpGet]
        public async Task<IActionResult> GetUsers(int page = 1)
        {
            int pageSize = 5;
            var users = _userManager.Users.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var viewModel = new List<MusterilerViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                viewModel.Add(new MusterilerViewModel
                {
                    UserId = user.Id,
                    Name = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles.ToList()
                });
            }

            return PartialView("_UserTablePartial", viewModel);
        }

        
        [HttpPost]
        public async Task<IActionResult> CreateUser(string name, string email, string phoneNumber, string password)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest(new { message = "Tüm alanlar doldurulmalıdır." });
            }

            var user = new IdentityUser
            {
                UserName = name,
                Email = email,
                PhoneNumber = phoneNumber
            };

            var result = await _userManager.CreateAsync(user, password);
           
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Musteri");
                return Json(new { message = "Kullanıcı başarıyla oluşturuldu." });
            }
            else
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { message = string.Join(", ", errors) });
            }


        }

        [HttpPost]
        
        public async Task<IActionResult> UpdateUserDetails(string userId, string name, string email, string phone, string role)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Kullanıcı Adı zorunludur.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            user.UserName = name ?? user.UserName;                       
            user.PhoneNumber = phone ?? user.PhoneNumber;
            user.Email = email ?? user.Email;                 

            var updateResult = await _userManager.UpdateAsync(user);

            if (updateResult.Succeeded)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!string.IsNullOrWhiteSpace(role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }

                return Ok("Kullanıcı bilgileri güncellendi.");
            }
            else
            {
                return BadRequest("Güncelleme işlemi başarısız.");
            }
        }

        [HttpGet]
        public IActionResult GetUserVehicles(string userId)
        {
            var vehicles = _context.Araclar
                .Where(v => v.SahipId == userId)
                .Select(v => new
                {
                    v.Plaka,
                    MarkaAdi = v.Marka.MarkaAdi,
                    ModelAdi = v.Model.ModelAdi,
                    v.Yil
                })
                .ToList();

            return Json(vehicles);
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var model = new MusterilerViewModel
            {
                UserId = user.Id,
                Name = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = roles.ToList()
            };

            return View(model); 
        }       

        public IActionResult Index()
        {
            var viewModel = new EminAutoAdminViewModel
            {                
                ToplamKampanyalar = _context.Kampanyalar.Count(k => k.BitisTarihi >= DateTime.Now),
                Kampanyalar = _context.Kampanyalar.Where(k => k.BitisTarihi >= DateTime.Now).ToList()
            };

            return View(viewModel);
        }


        public async Task<IActionResult> AktifKampanyalar()
        {
            var kampanyalar = await _context.Kampanyalar
                .OrderBy(k => k.BaslangicTarihi)
                .ToListAsync();

            return View(kampanyalar); 
        }
    }
}
