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
        public async Task<IEnumerable<Order>> UserOrders()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                throw new Exception("User is not logged in");

            var orders = await _db.Orders
                 .Include(x => x.OrderDetail)
                 .ThenInclude(x => x.Book)
                 .ThenInclude(x => x.Genre)
                 .Where(a => a.UserId == userId)
                 .ToListAsync();
                
            return orders;
        }

        private string GetUserId()
        {
            ClaimsPrincipal principal = _httpcontextAccessor.HttpContext.User;
            var userId = _userManager.GetUserId(principal);

            return userId;
        }
    }
}
