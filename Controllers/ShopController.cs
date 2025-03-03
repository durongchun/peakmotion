﻿using Microsoft.AspNetCore.Mvc;
using peakmotion.Data;
using peakmotion.Repositories;
using peakmotion.ViewModels;
using peakmotion.Models;
using System;
using System.Collections.Generic;

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

        // Action for displaying products
        public IActionResult Index()
        {
            IEnumerable<ShippingVM> shippings = _shopRepo.GetShippingInfo();
            var shippingInfo = shippings.FirstOrDefault();

            var productData = _cookieRepo.GetUserChosenProductInfoFromCookies();
            ViewData["ProductData"] = productData;

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
        public IActionResult PayPalConfirmation(
            string transactionId,
            string amount,
            string payerName,
            string email,
            string currency = "CAD")
        {
            // Safe conversion from string to decimal
            if (!decimal.TryParse(amount, out decimal parsedAmount))
            {
                parsedAmount = 0; // Fallback in case of failure
            }

            if (!long.TryParse(transactionId, out long parsedTransactionId))
            {
                // Handle the case where parsing fails
                parsedTransactionId = 0; // Or some default value
            }

            // Save confirmation to the database
            // var newPayPalConfirmation = new Order
            // {
            //     Pptransactionid = parsedTransactionId,
            //     // Amount = parsedAmount,
            //     // PayerName = payerName,
            //     // Email = email,
            //     Orderdate = DateOnly.FromDateTime(DateTime.UtcNow)
            // };

            // _context.Orders.Add(newPayPalConfirmation);
            // _context.SaveChanges();

            // Prepare ViewModel for the view
            var modelVM = new PayPalConfirmationVM
            {
                TransactionId = transactionId,
                Amount = parsedAmount,
                PayerName = payerName,
                Email = email,
                Currency = currency
            };

            _cookieRepo.RemoveCookie("ProductData");

            return View(modelVM);
        }
    }


}