using peakmotion.Data;
using peakmotion.ViewModels;
using peakmotion.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace peakmotion.Repositories
{
    public class PmuserRepo
    {
        private readonly PeakmotionContext _db;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PmuserRepo> _logger;

        public PmuserRepo(
            ILogger<PmuserRepo> logger,
            PeakmotionContext db,
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _db = db;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public int? GetUserIdByUserEmail(string email)
        {
            var userId = _db.Pmusers
                .Where(u => u.Email == email)
                .Select(u => u.Pkpmuserid)
                .FirstOrDefault();

            // result may be null - if not specified as int? the function can give a run-time error
            return userId;
        }

        public IdentityUser? GetUserByUserEmail(string email)
        {
            var user = _context.Users
                .Where(u => u.Email == email)
                .FirstOrDefault();
            return user;
        }

        /// <summary>
        ///     Get all the user's (should only have roles Employee, Admin, Customer)
        /// </summary>
        /// <returns></returns>
        public List<UserVM> GetAllEmployees()
        {
            List<UserVM> identityUsers = (from u in _context.Users
                                          join ur in _context.UserRoles on u.Id equals ur.UserId into mUser
                                          from ur in mUser.DefaultIfEmpty()
                                          join r in _context.Roles on ur.RoleId equals r.Id into mRole
                                          from r in mRole.DefaultIfEmpty()
                                          where r.Name == "Employee" || r.Name == "Admin" || r.Name == "Customer"
                                          select new UserVM
                                          {
                                              Email = u.Email,
                                              RoleName = r.Name ?? "No Role",
                                              FirstName = "",
                                              LastName = ""
                                          }).ToList();

            // Filter out the PMUsers that match the same emails with the target role
            var pmUsers = _db.Pmusers.ToList();
            foreach (var user in identityUsers)
            {
                var pmUser = pmUsers.FirstOrDefault(pm => pm.Email == user.Email);
                if (pmUser != null)
                {
                    user.FirstName = pmUser.Firstname;
                    user.LastName = pmUser.Lastname;
                }
            }
            Console.WriteLine($"DEBUG: Employee Count: {identityUsers.Count()}");
            return identityUsers;
        }

        public SelectList GetRoleSelectList()
        {
            var roles = _context.Roles.Select(r =>
                new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                })
                .ToList();

            return new SelectList(roles, "Value", "Text");
        }

        /// <summary>
        ///     Update a user's role
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        public async Task<(bool, string)> EditUserRole(string roleName, string userEmail)
        {
            (bool deleteRoleResult, string deleteRoleMsg) = await DeleteUserRole(roleName, userEmail);
            if (deleteRoleResult)
            {
                // Get the IdentityUser version - to update role
                IdentityUser? updatingUser = GetUserByUserEmail(userEmail);
                if (updatingUser == null) return (false, "error, Error updating the user's role");
                // Add the new role
                var updated = await _userManager.AddToRoleAsync(updatingUser, roleName);
                if (updated.Succeeded) return (true, $"success, Successfully updated the user's role: {userEmail} to '{roleName}'");
                return (false, $"error, Error updating the user's role: {userEmail} to '{roleName}'");
            }
            return (false, deleteRoleMsg);
        }

        /// <summary>
        ///     Delete a user's role
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        public async Task<(bool, string)> DeleteUserRole(string roleName, string userEmail)
        {
            // Check the current role exists
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists) return (false, "error, Error finding the current user's role"); // Role does not exist

            // Check the current user exists
            var identityUser = _httpContextAccessor.HttpContext?.User;
            IdentityUser? currentUser;
            if (identityUser != null)
            {
                currentUser = await _userManager.GetUserAsync(identityUser);
                if (currentUser == null) return (false, "error, Error finding the current user"); // Get the IdentityUser version - for email
            }
            else
            {
                return (false, "error, Error finding the current user context");
            }

            // Business Logic: Do not allow the current user to update themselves
            IdentityUser? updatingUser;
            if (userEmail.Equals(currentUser.Email))
            {
                Console.WriteLine("warning, Permission Denied: Cannot update your own role");
                return (false, "warning, Permission Denied: Cannot update your own role");
            }
            else
            {
                updatingUser = GetUserByUserEmail(userEmail);
                if (updatingUser == null) return (false, "error, Error finding the updated user"); // Get the IdentityUser version - update role
            }

            // Get the current role string
            var currentRoles = await _userManager.GetRolesAsync(updatingUser);
            string? currentRole;
            if (currentRoles.Count() > 0)
            {
                currentRole = currentRoles.FirstOrDefault();
                if (currentRole == null) return (false, "Error finding the updating user's role");
            }
            else
            {
                return (false, "error, Error the updating user seems to have no role");
            }

            // update the identity role
            var deleted = await _userManager.RemoveFromRoleAsync(updatingUser, currentRole);
            return (deleted.Succeeded, $"success, Successfully deleted the role '{roleName}' for {userEmail}");
        }

        public Pmuser? GetPmuserByEmail(string email)
        {
            return _db.Pmusers.FirstOrDefault(pm => pm.Email == email);
        }

        public void UpdatePmuser(Pmuser pmUser)
        {
            _db.Pmusers.Update(pmUser);
            _db.SaveChanges();
        }

        /// <summary>
        ///     Get PMUser by primary key ID (Pkpmuserid)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Pmuser? GetById(int id)
        {
            return _db.Pmusers.FirstOrDefault(pm => pm.Pkpmuserid == id);
        }
    }
}
