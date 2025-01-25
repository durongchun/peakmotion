using Microsoft.AspNetCore.Mvc;
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
        private readonly SessionRepo _sessionRepo;

        public ShopController(PeakmotionContext context, ShopRepo shopRepo, SessionRepo sessionRepo)
        {
            _shopRepo = shopRepo;
            _sessionRepo = sessionRepo;
            _context = context;
        }

        // Action for displaying products
        public IActionResult Index()
        {
            IEnumerable<ShippingVM> shippings = _shopRepo.GetShippingInfo();
            var shippingInfo = shippings.FirstOrDefault();

            var productData = _sessionRepo.GetUserChosenProductInfoFromCookies();
            ViewData["ProductData"] = productData;

            return View("Index", shippingInfo);
        }

        [HttpPost]
        public IActionResult Edit(string firstname, string lastname, string phone, string address, string city,
                                    string province, string postalcode, string country, string email)
        {
            string returnMessage = string.Empty;

            try
            {
                // Save shipping information using the repository
                _shopRepo.SaveShippingInfo(firstname, lastname, phone, address, city, province, postalcode, country, email);

                // Success message
                returnMessage = "Shipping information updated successfully!";
            }
            catch (Exception ex)
            {
                // Handle any errors during saving
                returnMessage = "An error occurred while updating the shipping information: " + ex.Message;
            }

            // Redirect to the Index action with the return message as a query parameter
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

            return View(modelVM);
        }
    }


}
