using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookShoppingCartMvcUI.Repositories
{
    public class UserOrderRepository: IUserOrderRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpcontextAccessor;
        private readonly UserManager<IdentityUser> _userManager;

        public UserOrderRepository(ApplicationDbContext db, IHttpContextAccessor httpcontextAccessor, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _httpcontextAccessor = httpcontextAccessor;
            _userManager = userManager;
        }

        public async Task ChangeOrderStatus(UpdateOrderStatusModel data)
        {
            var order = await _db.Orders.FindAsync(data.OrderId);

            if (order == null) 
            {
                throw new InvalidOperationException($"order with id {data.OrderId} does not found");
            }
            order.OrderStatusId = data.OrderStatusId;
            await _db.SaveChangesAsync();
        }

        public async Task<Order?> GetOrderById(int id)
        {
            return await _db.Orders.FindAsync(id);
        }

        public async Task<IEnumerable<OrderStatus>> GetOrderStatuses()
        {
            return await _db.orderStatuses.ToListAsync();
        }

        public async Task TogglePaymentStatus(int orderId)
        {
            var order = await _db.Orders.FindAsync(orderId);

            if (order == null)
            {
                throw new InvalidOperationException($"order with id:{orderId} does not found");
            }
            order.isPaid= !order.isPaid;
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Order>> UserOrders(bool getAll = false)
        {
            var orders = _db.Orders
                .Include(x=>x.OrderStatus)
                .Include(x=>x.OrderDetail)
                .ThenInclude(x=>x.Book)
                .ThenInclude(x=>x.Genre).AsQueryable();

            if (!getAll)
            {
                var userId = GetUserId();
                if(string.IsNullOrEmpty(userId))
                {
                    throw new Exception("User Is not logged-in");
                   
                }
                orders = orders.Where(a => a.UserId == userId);
                return await orders.ToListAsync();
            }

            return await orders.ToListAsync();
        }

        private string GetUserId()
        {
            ClaimsPrincipal principal = _httpcontextAccessor.HttpContext.User;
            var userId = _userManager.GetUserId(principal);

            return userId;
        }
    }
}
