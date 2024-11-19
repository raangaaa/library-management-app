using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Perpustakaan.Models;

public class ReturnModel
{
    [Key]
    public int Return_Id { get; set; }  
     
    // Foreign keys
    public int Borrow_Id { get; set; }
    public BorrowModel? Borrow { get; set; }
    public int Book_Id { get; set; }
    public BookModel? Book { get; set; }

    [DataType(DataType.Date)]  
    [Column(TypeName = "date")]
    public DateTime Return_Date { get; set; }
    public int Penalty { get; set; } 
}
