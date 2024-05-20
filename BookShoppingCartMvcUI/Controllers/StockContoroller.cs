using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Text;

namespace BookShoppingCartMvcUI.Controllers
{
    [Authorize(Roles = nameof(Roles.Admin))]
    public class StockContoroller : Controller
    {
        private readonly IStockRepository _stockRepo;
        public StockContoroller(IStockRepository stockRepo)
        {
            _stockRepo = stockRepo;
        }
        public async Task<IActionResult> Index(string sTerm="")
        {
            var stocks = await _stockRepo.GetStocks(sTerm);
            return View (stocks);
        }
    }
}
