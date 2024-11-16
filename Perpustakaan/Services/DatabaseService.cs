using Perpustakaan.Models;
using Microsoft.EntityFrameworkCore;

namespace Perpustakaan.Services;

public class DatabaseService : DbContext
{
    public DbSet<BookModel>? Books { get; set; }
    public DbSet<StudentModel>? Students { get; set; }
    public DbSet<BorrowBookModel>? BorrowBooks { get; set; }
    public DbSet<UserModel>? Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserModel>()
            .Property(u => u.Role)
            .HasConversion<string>();
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=localhost,1433;Database=Perpustakaan;User Id=sa;Password=Rangga#05;TrustServerCertificate=true;");
    }
}


