using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FluentValidation;

namespace Perpustakaan.Models;

public class BorrowModel
{
    [Key]
    public int Borrow_Id { get; set; }

    
    public int User_Id { get; set; }
    public UserModel? User { get; set; }
      
    [Required, Column(TypeName = "date"), DataType(DataType.Date)]
    public DateTime Borrow_Date { get; set; }

    [Required, Column(TypeName = "date"), DataType(DataType.Date)]
    public DateTime Return_Date { get; set; }

    public int Loan_Duration { get; set; }
    public int Penalty { get; set; } 

    public ICollection<BorrowBookModel>? BorrowBooks { get; set; }
    public ICollection<ReturnModel>? Returns { get; set; }

}

public class BorrowModelValidator : AbstractValidator<BorrowModel>
{
    public BorrowModelValidator()
    {
        RuleFor(b => b.User_Id).NotEmpty().GreaterThan(0);
        RuleFor(b => b.Borrow_Date).NotEmpty();
        RuleFor(b => b.Return_Date).NotEmpty();
        RuleFor(b => b.Loan_Duration).NotEmpty().GreaterThan(0);
        RuleFor(b => b.Penalty).NotEmpty().GreaterThan(0);
    }
}

