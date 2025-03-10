using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using peakmotion.Data;
using peakmotion.Models;

namespace peakmotion.Repositories
{
    public class OrderRepo
    {
        private readonly PeakmotionContext _context;

        public OrderRepo(PeakmotionContext context)
        {
            _context = context;
        }

        // Existing methods remain unchanged
        public async Task<List<Order>> GetAllOrders()
        {
            return await _context.Orders
                .Include(o => o.Fkpmuser)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Fkproduct)
                        .ThenInclude(p => p.Fkdiscount)
                .Include(o => o.OrderStatuses)
                .ToListAsync();
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
                .Include(o => o.Fkpmuser)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Fkproduct)
                .Include(o => o.OrderStatuses)
                .FirstOrDefaultAsync(o => o.Pkorderid == orderId);
        }

        public async Task<Order> UpdateOrder(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return order;
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

        // --- Newly Added Methods for User-Specific Order Retrieval ---

        // Get orders for a specific pmUserId, sorted in descending order
        public async Task<List<Order>> GetOrdersByPmUserId(int pmUserId)
        {
            return await _context.Orders
                .Where(o => o.Fkpmuserid == pmUserId)
                .Include(o => o.Fkpmuser)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Fkproduct)
                        .ThenInclude(p => p.Fkdiscount)
                .Include(o => o.OrderStatuses)
                .OrderByDescending(o => o.Pkorderid)
                .ToListAsync();
        }

        // Get a specific order for a user by orderId and pmUserId
        public async Task<Order> GetOrderByIdForUser(int orderId, int pmUserId)
        {
            return await _context.Orders
                .Where(o => o.Pkorderid == orderId && o.Fkpmuserid == pmUserId)
                .Include(o => o.Fkpmuser)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Fkproduct)
                .Include(o => o.OrderStatuses)
                .FirstOrDefaultAsync();
        }
    }
}
