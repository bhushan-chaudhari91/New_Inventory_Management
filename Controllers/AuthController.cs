using DocumentFormat.OpenXml.Spreadsheet;
using InventoryManagement.EntityModels;
using InventoryManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers
{
    public class AuthController : Controller
    {
        private readonly DbInventoryContext _context;

        public AuthController(DbInventoryContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserInfoViewModel user)
        {
            var checkUse = await _context.TblUsers
                .FirstOrDefaultAsync(x => x.UserName == user.UserName && x.Password == user.Password && x.IsDeleted == false);

            if(checkUse != null)
            {
                HttpContext.Session.SetInt32("userId", (int)checkUse.UserId);
                HttpContext.Session.SetString("userName", checkUse.UserName);
                HttpContext.Session.SetInt32("FkRole", (int)checkUse.FkRoleId);
                return RedirectToAction("InventoryList", "Inventory");
            }

            ModelState.AddModelError(string.Empty, "Please enter the correct username & password.");
            return View(user);

        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Auth");
        }

        public IActionResult Warehouse()
        {
            int? role = HttpContext.Session.GetInt32("FkRole");
            if (role != 1)
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            return View();
        }

    }
}
