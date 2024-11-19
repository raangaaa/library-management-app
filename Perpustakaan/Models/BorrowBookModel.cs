using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Perpustakaan.Models;

public class BorrowBookModel
{
    [Key]
    public int Borrow_Book_Id { get; set; }  
     
    // Foreign keys
    public int Borrow_Id { get; set; }
    public BorrowModel? Borrow { get; set; }
    public int Book_Id { get; set; }
    public BookModel? Book { get; set; }
    
    public int Borrow_Amount { get; set; }
}
