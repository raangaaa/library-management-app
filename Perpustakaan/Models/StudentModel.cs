using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Perpustakaan.Models;

public class StudentModel
{
    [Key]
    public string? NIS { get; set; }
    public string? Name { get; set; }
    public string? Class { get; set; } 
    public string? Address { get; set; }
    public int Phone { get; set; } 

    public ICollection<BorrowBookModel>? BorrowBooks { get; set; }
}
