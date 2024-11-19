using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Perpustakaan.Models;

public class BookModel
{
    [Key]
    public int Book_Id { get; set; }
    [MaxLength(30)]
    public string? Title { get; set; }
    [MaxLength(20)]
    public string? Author { get; set; }
    [MaxLength(20)]
    public string? Publisher { get; set; } 
    public int Year { get; set; }
    public int Stock { get; set; } 

    public ICollection<BorrowBookModel>? BorrowBooks { get; set; }
    public ICollection<ReturnModel>? Returns { get; set; }
}
