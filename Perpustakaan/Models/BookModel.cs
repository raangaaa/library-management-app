using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Perpustakaan.Models;

public class BookModel
{
    [Key]
    public int BookId { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Publisher { get; set; } 
    public int Year { get; set; }
    public int Stock { get; set; } 

    public ICollection<BorrowBookModel>? BorrowBooks { get; set; }
}
