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

        public PmuserRepo(PeakmotionContext db, ApplicationDbContext context)
        {
            _context = context;
            _db = db;
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

        public List<UserVM> GetAllEmployees()
        {
            List<UserVM> identityUsers = (from u in _context.Users
                                          join ur in _context.UserRoles on u.Id equals ur.UserId into mUser
                                          from ur in mUser.DefaultIfEmpty()
                                          join r in _context.Roles on ur.RoleId equals r.Id into mRole
                                          from r in mRole.DefaultIfEmpty()
                                          where r.Name == "Employee" || r.Name == "Admin"
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

        public Pmuser? GetPmuserByEmail(string email)
        {
            return _db.Pmusers.FirstOrDefault(pm => pm.Email == email);
        }

        public void UpdatePmuser(Pmuser pmUser)
        {
            _db.Pmusers.Update(pmUser);
            _db.SaveChanges();
        }
    }
}
