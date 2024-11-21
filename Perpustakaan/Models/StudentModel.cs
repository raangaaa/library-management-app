using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FluentValidation;
using Microsoft.EntityFrameworkCore;


namespace Perpustakaan.Models;


[Index(nameof(NIS), IsUnique = true)]
public class StudentModel
{
    [Key]
    public string? Student_Id { get; set; }

    public int User_Id { get; set; } // Foreign Key

    public UserModel? User { get; set; }

    [Required, MaxLength(20)]
    public string? NIS { get; set; }

    [Required, MaxLength(15)]
    public string? Class { get; set; }
    
    [Required, Column(TypeName = "text")]
    public string? Address { get; set; }
}

public class StudentModelValidator : AbstractValidator<StudentModel>
{
    public StudentModelValidator()
    {
        RuleFor(s => s.NIS).NotEmpty().MaximumLength(20);
        RuleFor(s => s.Class).NotEmpty().MaximumLength(15);
        RuleFor(s => s.Address).NotEmpty();
    }
}
