using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Perpustakaan.Models;

public class BookModel
{
    [Key]
    public int Book_Id { get; set; }
    [Required, MaxLength(30)]
    public string? Title { get; set; }
    [Required, MaxLength(20)]
    public string? Author { get; set; }
    [Required, MaxLength(20)]
    public string? Publisher { get; set; } 
    public int Year { get; set; }
    public int Stock { get; set; } 

    public ICollection<BorrowBookModel>? BorrowBooks { get; set; }
    public ICollection<ReturnModel>? Returns { get; set; }
}

public class BookModelValidator : AbstractValidator<BookModel>
{
    public BookModelValidator()
    {
        RuleFor(b => b.Title).NotEmpty().MaximumLength(30);
        RuleFor(b => b.Author).NotEmpty().MaximumLength(20);
        RuleFor(b => b.Publisher).NotEmpty().MaximumLength(20);
        RuleFor(b => b.Year).NotEmpty().GreaterThan(0);
        RuleFor(b => b.Stock).NotEmpty().GreaterThan(0);
    }
}

