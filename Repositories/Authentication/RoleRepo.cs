using Microsoft.AspNetCore.Identity;
using peakmotion.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace peakmotion.Repositories
{
    public class RoleRepo
    {
        private readonly ApplicationDbContext _db;

        public RoleRepo(ApplicationDbContext db)
        {
            _db = db;
        }

        public IEnumerable<IdentityRole> GetAllRoles()
        {
            var roles = _db.Roles.ToList();

            return roles;
        }

        public IdentityRole? GetRole(string roleName)
        {
            IdentityRole? role =
            _db.Roles.FirstOrDefault(r => r.Name == roleName);

            return role;
        }

        public bool DoesRoleHaveUsers(string roleName)
        {
            IdentityRole? role = GetRole(roleName);
            if (role == null)
            {
                return false;
            }
            return
                _db.UserRoles.Any(r => r.RoleId == role.Id);
        }

        public SelectList GetRoleSelectList()
        {
            var roles = GetAllRoles().Select(r =>
                new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                }).ToList();

            SelectList roleSelectList =
            new SelectList(roles, "Value", "Text");

            return roleSelectList;
        }

    }
}
