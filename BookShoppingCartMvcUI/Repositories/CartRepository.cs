using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace BookShoppingCartMvcUI.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpcontextAccessor;
        public CartRepository(ApplicationDbContext db, UserManager<IdentityUser> userManager, IHttpContextAccessor HttpcontextAccessor)
        {
            _db = db;
            _userManager = userManager;
            _httpcontextAccessor = HttpcontextAccessor;
        }
        public async Task<int> AddItem(int bookId, int qty)
        {
            string userId = GetUserId();
            using var transaction = _db.Database.BeginTransaction();
            try {
                

                if (string.IsNullOrEmpty(userId))
                    throw new Exception("user is not logged-in");



                var cart = await GetCart(userId);
                if (cart is null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId,
                    };
                    _db.ShoppingCarts.Add(cart);
                }
                _db.SaveChanges();
                var cartItem = _db.CartDetails.FirstOrDefault(x => x.ShoppingCartId == cart.Id && x.BookId == bookId);
                if (cartItem is not null)
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    var book = _db.Books.Find(bookId);
                    cartItem = new CartDetail
                    {
                        BookId = bookId,
                        ShoppingCartId = cart.Id,
                        Quantity = qty,
                        UnitPrice = book.Price
                    };

                    _db.CartDetails.Add(cartItem);
                }
                _db.SaveChanges();
                transaction.Commit();
                
            }
            catch (Exception ex)
            {
                
            }
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }

        public async Task<int> RemoveItem(int bookId)
        {
          
            using var transaction = _db.Database.BeginTransaction();
            string userId = GetUserId();
            try
            {


                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not loggged-in");



                var cart = await GetCart(userId);
                if (cart is null)
                {
                    throw new Exception("Invaalid cart");
                }

                var cartItem = _db.CartDetails.FirstOrDefault(x => x.ShoppingCartId == cart.Id && x.BookId == bookId);

                if (cartItem is null)
                {
                    throw new Exception("Not items in cart ");
                }

                else if (cartItem.Quantity == 1)
                {
                    _db.CartDetails.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = cartItem.Quantity - 1;



                }
                _db.SaveChanges();
                //transaction.Commit();
              
            }
            catch (Exception ex)
            {
                
            }
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;

        }

        public async Task<ShoppingCart>GetUserCart()
        {
            var userId = GetUserId();

            if(userId == null)
            {
                throw new Exception("Invalid userid");
            }
            var shoppingCart = await _db.ShoppingCarts
                                       .Include(x => x.CartsDetails)
                                       .ThenInclude(x => x.Book)
                                       .ThenInclude(x => x.Genre)
                                       .Where(a => a.UserId == userId).FirstOrDefaultAsync();
            return shoppingCart;
        }


        public async Task<ShoppingCart> GetCart(string userId) 
        {
            var cart = await _db.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);
            return cart;
        }
        
        
        public async Task<int> GetCartItemCount(string userid="")
        {

            if (string.IsNullOrEmpty(userid))//fix
            {
                userid= GetUserId();
            }
            var data = await (from cart in _db.ShoppingCarts
                              join CartDetail in _db.CartDetails
                              on cart.Id equals CartDetail.ShoppingCartId
                              where cart.UserId == userid //fix
                              select new { CartDetail.Id }
                              ).ToListAsync();
            return data.Count;
        }
        
        public async Task<bool> DoCheck(CheckoutModel model)
        {
            using var transaction = _db.Database.BeginTransaction();

            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged-in");
                var cart = await GetCart(userId);
                if (cart is null)
                throw new Exception("Invalid cart");

                var cartDetail = _db.CartDetails.Where(c => c.ShoppingCartId == cart.Id).ToList();

                if (cartDetail.Count == 0)
                {
                    throw new Exception("Cart is Empty");
                }

                var order = new Order
                {
                    UserId = userId,
                    CreatedDate = DateTime.UtcNow,
                    OrderStatusId = 1,

                };
                _db.Orders.Add(order);
                _db.SaveChanges();
                foreach (var item in cartDetail)
                {
                    var orderDetail = new OrderDetail
                    {
                        BookId = item.BookId,
                        OrderId = order.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                    };
                    _db.OrderDetails.Add(orderDetail);

                }
                _db.SaveChanges();

                _db.CartDetails.RemoveRange(cartDetail);
                _db.SaveChanges();

                transaction.Commit();

                return  true;


            }
            catch (Exception)
            {

                throw;
            }

        }

        private string GetUserId()
        {
            ClaimsPrincipal principal = _httpcontextAccessor.HttpContext.User;
            var userId = _userManager.GetUserId(principal);

            return userId;
        }
    }
}
