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
            // IMPORTANT: Include Fkpmuser for buyer's info, and include Fkproduct for product names
            return await _context.Orders
                .Include(o => o.Fkpmuser)  // Load buyer info
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Fkproduct)  // Load product info
                .Include(o => o.OrderStatuses)
                .FirstOrDefaultAsync(o => o.Pkorderid == orderId);
        }

        public async Task<List<Order>> GetAllOrders()
        {
            // If you also need buyer info and product info in listing, do the same Includes
            return await _context.Orders
                .Include(o => o.Fkpmuser)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Fkproduct)
                .Include(o => o.OrderStatuses)
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
        public async Task<List<Order>> GetOrdersByUserId(string userId)
        {
            return await _context.Orders
                .Where(o => o.Fkpmuserid.ToString() == userId)
                .ToListAsync();
        }

        // Get a specific order for a user by order ID
        public async Task<Order> GetOrderByIdForUser(int orderId, string userId)
        {
            return await _context.Orders
                .Where(o => o.Fkpmuserid.ToString() == userId && o.Pkorderid == orderId)
                .Include(o => o.OrderStatuses)
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Fkproduct)
                .FirstOrDefaultAsync();
        }
    }
}
