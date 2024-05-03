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

        public Task ChangeOrderStatus(UpdateOrderStatusModel data)
        {
            throw new NotImplementedException();
        }

        public Task<Order?> GetOrderById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<OrderStatus>> GetOrderStatuses()
        {
            throw new NotImplementedException();
        }

        public Task TogglePaymentStatus(int orderId)
        {
            throw new NotImplementedException();
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
