﻿using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagement.ViewModels
{
    public class UserViewModel
    {
        public int UserId { get; set; }

        public string FullName { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        public string ContactNo { get; set; }

        public string Address { get; set; }

        public int FkRoleId { get; set; }

        public string Gender { get; set; }

        


    }

    public class UserListViewModel
    {
        public List<UserViewModel> users { get; set; }
        public PaginationMetadataViewModel Pagination { get; set; }
        public int FkRoleId { get; set; }
        public SelectList RoleNameList { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
    }
}
