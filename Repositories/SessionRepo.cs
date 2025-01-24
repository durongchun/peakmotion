using peakmotion.Data;
using peakmotion.ViewModels;
using peakmotion.Models;
using Microsoft.EntityFrameworkCore;

namespace peakmotion.Repositories
{
  public class SessionRepo
  {
    private readonly HttpContext _context;

    public SessionRepo(HttpContext context)
    {
      _context = context;
    }

    public void AddSession(){
      //var session = HttpContext.Session.SessionID;
      var sessionID = _context.Session;
    }

    // public void UpdateSession(_sessionID){
    //   // Update session
    // }

    // public void DeleteSession(_sessionID){
    //   // Delete session 
    //   //HTTPContext.Session.Clear(_sessionID);
    // }

  }
}