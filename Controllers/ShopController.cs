using Microsoft.AspNetCore.Mvc;
using peakmotion.Data;
using peakmotion.Repositories;
using peakmotion.ViewModels;
using peakmotion.Models;
using System;
using System.Collections.Generic;
using NuGet.Protocol;

namespace peakmotion.Controllers
{
    public class ShopController : Controller
    {
        private readonly ShopRepo _shopRepo;
        private readonly PeakmotionContext _context;
        private readonly CookieRepo _cookieRepo;

        public ShopController(PeakmotionContext context, ShopRepo shopRepo, CookieRepo cookieRepo)
        {
            _shopRepo = shopRepo;
            _cookieRepo = cookieRepo;
            _context = context;
        }


        public IActionResult Index()
        {
            IEnumerable<ShippingVM> shippings = _shopRepo.GetShippingInfo();
            var shippingInfo = shippings.FirstOrDefault();

            var totalAmount = _shopRepo.GetTotalAmount();

            var productData = _cookieRepo.GetUserChosenProductInfoFromCookies();

            ViewData["ProductData"] = productData;
            ViewData["TotalAmount"] = totalAmount;

            return View("Index", shippingInfo);
        }

        [HttpPost("Shipping/Edit")]
        public IActionResult Edit(ShippingVM model)
        {
            string returnMessage = string.Empty;

            try
            {
                if (model.IsSaveAddress)
                {
                    _shopRepo.SaveShippingInfo(model);
                    returnMessage = "Shipping information updated successfully!";
                }
            }
            catch (Exception ex)
            {
                returnMessage = "An error occurred while updating the shipping information: " + ex.Message;
            }

            return RedirectToAction("Index", new { message = returnMessage });
        }



        [HttpGet("Home/PayPalConfirmation")]
        public IActionResult PayPalConfirmation(PayPalConfirmationVM model)
        {
            var modelVM = new PayPalConfirmationVM
            {
                TransactionId = model.TransactionId,
                Amount = model.Amount,
                PayerName = model.PayerName,
                Email = model.Email,
                Currency = model.Currency,
            };

            _shopRepo.SaveOrderInfo(model);
            _shopRepo.SaveOrderStatus(model);
            _shopRepo.SaveOrderProduct(model);
            _shopRepo.UpdateProductStock();

            _cookieRepo.RemoveCookie("ProductData");
            _cookieRepo.RemoveCookie("cart");


            return View(modelVM);
        }
    }


}