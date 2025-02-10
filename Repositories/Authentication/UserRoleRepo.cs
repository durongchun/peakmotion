using Microsoft.AspNetCore.Identity;
using peakmotion.ViewModels;

public class UserRoleRepo
{
    private readonly UserManager<IdentityUser> _userManager;

    public UserRoleRepo(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> AddUserRoleAsync(string email, string roleName)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return false;
        }

        var result = await _userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded;
    }


    public async Task<bool> RemoveUserRoleAsync(string email, string roleName)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return false;
        }

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        return result.Succeeded;
    }
}
