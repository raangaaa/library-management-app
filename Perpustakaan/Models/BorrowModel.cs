using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Perpustakaan.Models;

public class BorrowModel
{
    [Key]
    public int Borrow_Id { get; set; }

    
    public int User_Id { get; set; }
    public UserModel? User { get; set; }

    [DataType(DataType.Date)]  
    [Column(TypeName = "date")]
    public DateTime Borrow_Date { get; set; }

    [DataType(DataType.Date)]  
    [Column(TypeName = "date")]
    public DateTime Return_Date { get; set; }

    public int Loan_Duration { get; set; }
    public int Penalty { get; set; } 

    public ICollection<BorrowBookModel>? BorrowBooks { get; set; }
    public ICollection<ReturnModel>? Returns { get; set; }

}
