using peakmotion.Data;
using peakmotion.ViewModels;
using peakmotion.Models;
using Microsoft.EntityFrameworkCore;
namespace peakmotion.Repositories
{
    public class OrderRepo
    {
        private readonly PeakmotionContext _context;
        public OrderRepo(PeakmotionContext context)
        {
            _context = context;
        }
        public async Task<Order> CreateOrder(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }
        public async Task<Order> GetOrderById(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderProducts)
                .Include(o => o.OrderStatuses)
                .FirstOrDefaultAsync(o => o.Pkorderid == orderId);
        }
        public async Task<List<Order>> GetAllOrders()
        {
            return await _context.Orders
                .Include(o => o.OrderProducts)
                .Include(o => o.OrderStatuses)
                .Include(o => o.Fkpmuser)
                .ToListAsync();
        }
        public async Task DeleteOrder(int orderId)
        {
            var order = await GetOrderById(orderId);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<Order> UpdateOrder(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }
        // Get all order statuses for a specific order
        public async Task<List<OrderStatus>> GetOrderStatuses(int orderId)
        {
            return await _context.OrderStatuses
                  .Where(os => os.Fkorderid == orderId)
                  .ToListAsync();
        }


        // Get all orders for a specific user
        public async Task<List<Order>> GetOrdersByUserId(int userId)
        {
            return await _context.Orders
                .Where(o => o.Fkpmuserid == userId)  // Adjusted to use string comparison
                .ToListAsync();
        }


        // Get a specific order for a user by order ID
        public async Task<Order> GetOrderByIdForUser(int orderId, int userId)
        {
            return await _context.Orders
                .Where(o => o.Fkpmuserid == userId && o.Pkorderid == orderId)
                .Include(o => o.OrderStatuses)
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Fkproduct)
                .FirstOrDefaultAsync();
        }
    }
}