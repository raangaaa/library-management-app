using System.ComponentModel.DataAnnotations;

namespace Perpustakaan.Models;

public enum Roles {
    Admin = 1,
    Student = 2,
}

public class UserModel
{
    [Key]
    public int UserId { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public Roles Role { get; set; } 
}
