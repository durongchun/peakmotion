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
        private readonly ProductRepo _productRepo;
        private readonly PeakmotionContext _context;

        public ShopController(PeakmotionContext context, ProductRepo productRepo)
        {
            _productRepo = productRepo;
            _context = context;
        }

        // Action for displaying products
        public IActionResult Index()
        {
            IEnumerable<ProductVM> products = _productRepo.GetAllProducts();
            Console.WriteLine($"Number of products: {products.Count()}");
            return View("Index", products);
        }


        [HttpGet("Shop/PayPalConfirmation")]
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
            var newPayPalConfirmation = new Order
            {
                Pptransactionid = parsedTransactionId,
                // Amount = parsedAmount,
                // PayerName = payerName,
                // Email = email,
                Orderdate = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            _context.Orders.Add(newPayPalConfirmation);
            _context.SaveChanges();

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
