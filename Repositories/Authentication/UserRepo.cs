using Microsoft.AspNetCore.Mvc.Rendering;
using peakmotion.Data;
using peakmotion.ViewModels;
public class UserRepo
{
    private readonly ApplicationDbContext _db;

    public UserRepo(ApplicationDbContext db)
    {
        _db = db;
    }

    public IEnumerable<UserVM> GetAllUsers()
    {
        IEnumerable<UserVM> users = _db.Users.Select(u =>
            new UserVM { Email = u.Email }).ToList();

        return users;
    }

    public SelectList GetUserSelectList(string? email)
    {
        IEnumerable<SelectListItem> users =
            GetAllUsers().Select(u => new SelectListItem
            {
                Value = u.Email,
                Text = u.Email
            });

        SelectList roleSelectList =
            new SelectList(users, "Value", "Text", email);

        return roleSelectList;
    }
}
