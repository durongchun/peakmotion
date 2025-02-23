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

        public PmuserRepo(ILogger<PmuserRepo> logger, PeakmotionContext db, ApplicationDbContext context,
        UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IHttpContextAccessor httpContextAccessor)
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
                                              FirstName = "", // fill later
                                              LastName = ""   // fill later
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

        async public Task<bool> EditUserRole(string roleName)
        {
            // Check the role and user exists
            var identityUser = _httpContextAccessor.HttpContext?.User;
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if ((identityUser != null) && roleExists)
            {
                // get the IdentityUser type
                IdentityUser? currentUser = await _userManager.GetUserAsync(identityUser);
                if (currentUser != null)
                {
                    // get the current role string
                    var currentRoles = await _userManager.GetRolesAsync(currentUser);
                    var currentRole = currentRoles.FirstOrDefault();
                    if (currentRole != null && currentRole.Count() > 0)
                    {
                        // update the identity role
                        var deleted = await _userManager.RemoveFromRoleAsync(currentUser, currentRole);
                        if (deleted.Succeeded)
                        {
                            var updated = await _userManager.AddToRoleAsync(currentUser, roleName);
                            if (updated.Succeeded)
                            {
                                return true;
                            }
                        }
                    }

                }
            }

            return false;
        }
    }
}
