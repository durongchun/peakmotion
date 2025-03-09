using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using peakmotion.Models;
namespace peakmotion.ViewModels
{
    public class OrderVM
    {
        public int OrderId { get; set; }
        public string customerName { get; set; }
        public string Email { get; set; }
        public DateOnly OrderDate { get; set; }
        public decimal Total { get; set; }
        public string ShippingStatus { get; set; }
        public DateOnly? ShippedDate { get; set; }
        public DateOnly? DeliveryDate { get; set; }
        public string Pptransactionid { get; set; }
        public List<OrderItemVM> Items { get; set; } = new List<OrderItemVM>();
    }
    public class OrderCreateVM
    {
        public DateOnly OrderDate { get; set; }
        public int Fkpmuserid { get; set; }
        public List<OrderProductVM> OrderProducts { get; set; } = new List<OrderProductVM>();
    }
    public class OrderProductVM
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Unitprice { get; set; }
    }
    public class OrderStatusVM
    {
        public string Status { get; set; }
        public DateOnly StatusDate { get; set; }
    }
    public class OrderItemVM
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Unitprice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
