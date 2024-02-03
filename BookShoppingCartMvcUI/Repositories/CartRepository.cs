using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookShoppingCartMvcUI.Repositories
{
    public class CartRepository
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
        public  async Task<bool> AddItem(int bookId,int qty)
        {
            using var transaction = _db.Database.BeginTransaction();
            try {
                string userId = GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return false;



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
                var cartItem = _db.CartDetails.FirstOrDefault(x => x.ShoppingCartId ==cart.Id && x.BookId==bookId);
                if (cartItem is not null)
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    cartItem = new CartDetail 
                    {
                        BookId = bookId,
                        ShoppingCartId=cart.Id,
                        Quantity=qty
                    };

                    _db.CartDetails.Add(cartItem);
                }
                _db.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> RemoveItem(int bookId)
        {
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                string userId = GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return false;



                var cart = await GetCart(userId);
                if (cart is null)
                {
                    return false;
                }
                
                var cartItem = _db.CartDetails.FirstOrDefault(x => x.ShoppingCartId == cart.Id && x.BookId == bookId);
                
                if(cartItem is null)
                {
                    return false;
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
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<ShoppingCart> GetCart(string userId) 
        {
            var cart = await _db.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);
            return cart;
        }
        private string GetUserId()
        {
            ClaimsPrincipal principal = _httpcontextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);

            return userId;
        }
    }
}
