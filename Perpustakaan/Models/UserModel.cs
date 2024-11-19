using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Perpustakaan.Models;

public enum Roles {
    Admin = 1,
    Student = 2,
}

public class UserModel
{
    [Key]
    public int User_Id { get; set; }

    [MaxLength(25)]
    public string? Name { get; set; }

    public Roles Role { get; set; } 

    [MaxLength(25)]
    public string? Email { get; set; }

    [MaxLength(15)]
    public string? No_Telp { get; set; }

    [MaxLength(20)]
    public string? Username { get; set; }
    
    [MaxLength(20)]
    public string? Password { get; set; }

    public StudentModel? Student { get; set; }
    public ICollection<BorrowModel>? Borrows { get; set; } 
}
