using System.ComponentModel.DataAnnotations;

namespace Perpustakaan.Models;

public class BorrowBookModel
{
    [Key]
    public int Id { get; set; }  
     
    // Foreign keys
    public int NIS { get; set; }
    public StudentModel? Student { get; set; }

    public int BookId { get; set; }
    public BookModel? Book { get; set; }
}
