using Microsoft.EntityFrameworkCore;

namespace BookShoppingCartMvcUI.Repositories
{
    public class StockRepository
    {
        private readonly ApplicationDbContext _context;

        public StockRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Stock?>GetStockByBookId(int bookId)=> await _context.Stocks.FirstOrDefaultAsync(a=>a.BookId == bookId);

        public async Task<IEnumerable<StockDisplayModel>>GetStocks(string sTerm = "")
        {
            var stocks = await (from book in _context.Books
                                join stock in _context.Stocks
                                on book.Id equals stock.BookId
                                into book_stock
                                from bookStock in book_stock.DefaultIfEmpty()
                                where string.IsNullOrWhiteSpace(sTerm) || book.BookName.ToLower().Contains(sTerm.ToLower())
                                select new StockDisplayModel
                                {
                                    BookId = book.Id,
                                    BookName = book.BookName,
                                    Quantity = bookStock == null ? 0 : bookStock.Quantity

                                }).ToListAsync();
            return stocks;
        }
    }
}
