using peakmotion.Data;
using peakmotion.ViewModels;
using peakmotion.Models;
using Microsoft.EntityFrameworkCore;

namespace peakmotion.Repositories
{
    public class PmuserRepo
    {
        private readonly PeakmotionContext _context;

        public PmuserRepo(PeakmotionContext context)
        {
            _context = context;
        }

        public int GetUserIdByUserEmail(string email)
        {
            var userId = _context.Pmusers
                .Where(u => u.Email == email)
                .Select(u => u.Pkpmuserid)
                .FirstOrDefault();

            return userId;
        }
    }
}
