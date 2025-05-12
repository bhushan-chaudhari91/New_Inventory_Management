using InventoryManagement.EntityModels;
using InventoryManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers
{
    public class UserController : Controller
    {
        private readonly DbInventoryContext _context;

        public UserController(DbInventoryContext context)
        {
            _context = context;
        }


        public IActionResult UserList(string searchTerm = "", int pageNumber = 1, int pageSize = 10)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            List<UserViewModel> getUserList = new List<UserViewModel>();

            var userList = _context.TblUsers.Where(x => x.IsDeleted == false && x.UserId != 0).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                userList = userList.Where(x => x.UserName.Contains(searchTerm) ||
                                               x.FullName.Contains(searchTerm) ||
                                               x.Address.Contains(searchTerm) ||
                                               x.Email.Contains(searchTerm) ||
                                               x.ContactNo.Contains(searchTerm));
            }


            int totalRecords = userList.Count();

            var paginatedUsers= userList
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .ToList();

            foreach (var item in paginatedUsers)
            {

                getUserList.Add(new UserViewModel
                {
                    UserId = item.UserId,
                    FullName = item.FullName,
                    UserName = item.UserName,
                    Email = item.Email,
                    ContactNo = item.ContactNo,
                    Address = item.Address
                });
            }

            var viewModel = new UserListViewModel
            {
                users = getUserList,
                Pagination = new PaginationMetadataViewModel
                {
                    TotalRecords = totalRecords,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    SearchTerm = searchTerm
                }
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(UserViewModel addUser)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = new TblUser
            {
                UserName = addUser.UserName,
                FullName = addUser.FullName,
                Password = addUser.Password,
                Email = addUser.Email,
                ContactNo = addUser.ContactNo,
                Address = addUser.Address,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };

            _context.TblUsers.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("UserList");
        }


        [HttpPost]
        public async Task<IActionResult> EditUser(UserViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var existingUser = await _context.TblUsers.FindAsync(model.UserId);

            if (existingUser == null)
                return NotFound();

            existingUser.FullName = model.FullName;
            existingUser.UserName = model.UserName;
            existingUser.Password = model.Password;
            existingUser.Email = model.Email;
            existingUser.ContactNo = model.ContactNo;
            existingUser.Address = model.Address;
            existingUser.UpdatedAt = DateTime.Now;

            _context.TblUsers.Update(existingUser);
            await _context.SaveChangesAsync();

            return RedirectToAction("UserList");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _context.TblUsers.FirstOrDefaultAsync(x => x.UserId == id);

            if (user == null)
            {
                throw new Exception("Supplier not found");
            }

            user.IsDeleted = true;
            _context.TblUsers.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("UserList", "User");
        }

    }
}
