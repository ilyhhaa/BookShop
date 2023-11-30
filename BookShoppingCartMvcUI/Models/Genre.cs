using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BookShoppingCartMvcUI.Models
{
    [Table("Genre")]
    public class Genre
    {
        
        
        
            public int Id { get; set; }

            [Required] //свойство должно быть обязательно установлено
            [MaxLength(40)]
            public string GenreName { get; set; }

            public List<Book> Books { get; set;}
          


        
    }
}
