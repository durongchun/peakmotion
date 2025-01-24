using System.Collections;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using peakmotion.Repositories;
using peakmotion.ViewModels;
using peakmotion.Models;
using peakmotion.Data;
using Microsoft.EntityFrameworkCore;

namespace peakmotion.Controllers;

public class CartController : Controller
{
    private readonly SessionRepo _sessionRepo;
    //private readonly PeakmotionContext _context;

    public CartController(SessionRepo sessionRepo)
    {
        _sessionRepo = sessionRepo;
    }
}     