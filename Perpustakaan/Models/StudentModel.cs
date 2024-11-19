using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace Perpustakaan.Models;


[Index(nameof(NIS), IsUnique = true)]
public class StudentModel
{
    [Key]
    public string? Student_Id { get; set; }

    public int User_Id { get; set; } // Foreign Key

    public UserModel? User { get; set; }

    [MaxLength(20)]
    public string? NIS { get; set; }

    [MaxLength(15)]
    public string? Class { get; set; }
    
    [Column(TypeName = "text")]
    public string? Address { get; set; }
}
