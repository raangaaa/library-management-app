using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Perpustakaan.Models;

public enum Roles {
    Admin = 1,
    Student = 2,
}

public class UserModel
{
    [Key]
    public int User_Id { get; set; }

    [Required, MaxLength(25)]
    public string? Name { get; set; }

    public Roles Role { get; set; } 

    [Required, MaxLength(25), EmailAddress]
    public string? Email { get; set; }

    [Required, MaxLength(15), Phone]
    public string? Phone { get; set; }

    [Required, MaxLength(20)]
    public string? Username { get; set; }
    
    [Required, MaxLength(20)]
    public string? Password { get; set; }

    public StudentModel? Student { get; set; }
    public ICollection<BorrowModel>? Borrows { get; set; } 
}

public class UserModelValidator : AbstractValidator<UserModel>
{
    public UserModelValidator()
    {
        RuleFor(u => u.Name).NotEmpty().MaximumLength(25);
        RuleFor(u => u.Email).NotEmpty().MaximumLength(25).EmailAddress();
        RuleFor(u => u.Phone).NotEmpty().MaximumLength(15).Matches(@"^\d+$").WithMessage("Phone must be numeric");
        RuleFor(u => u.Username).NotEmpty().MaximumLength(20);
        RuleFor(u => u.Password).NotEmpty().MaximumLength(20);
        RuleFor(u => u.Student).SetValidator(new StudentModelValidator());
    }
}

