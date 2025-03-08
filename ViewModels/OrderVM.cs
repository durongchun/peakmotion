using System.ComponentModel.DataAnnotations;
using peakmotion.Models;
namespace peakmotion.ViewModels
{
    public class OrderVM
    {
        public int OrderId { get; set; }
        public string customerName { get; set; }
        public DateOnly OrderDate { get; set; }
        public decimal Total { get; set; }
        public string ShippingStatus { get; set; }
        public List<OrderStatusVM> orderStatuses { get; set; } = new List<OrderStatusVM>();
        public List<OrderProductVM> OrderProducts { get; set; } = new List<OrderProductVM>();
        public List<Pmuser> Pmuser { get; set; } = new List<Pmuser>();
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
        public string ProductName { get; set; }
    }
    public class OrderStatusVM
    {
        public string Status { get; set; }
        public DateOnly StatusDate { get; set; }
    }
}