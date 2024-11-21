using Perpustakaan.Models;
using Microsoft.EntityFrameworkCore;

namespace Perpustakaan.Services;

public class DatabaseService : DbContext
{
    public DbSet<UserModel>? Users { get; set; }
    public DbSet<StudentModel>? Students { get; set; }
    public DbSet<BookModel>? Books { get; set; }
    public DbSet<BorrowModel>? Borrows { get; set; }
    public DbSet<BorrowBookModel>? BorrowBooks { get; set; }
    public DbSet<ReturnModel>? Return { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Model
        modelBuilder.Entity<UserModel>()
            .HasKey(u => u.User_Id); 

        modelBuilder.Entity<UserModel>()
            .Property(u => u.User_Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<UserModel>()
            .Property(u => u.Role)
            .HasConversion<string>();

        // Student Model
        modelBuilder.Entity<StudentModel>()
            .HasKey(u => u.Student_Id); 

        modelBuilder.Entity<StudentModel>()
            .Property(u => u.Student_Id)
            .ValueGeneratedOnAdd();
        
        modelBuilder.Entity<StudentModel>()
            .HasIndex(u => u.NIS)
            .IsUnique();

        // Book Model
        modelBuilder.Entity<BookModel>()
            .HasKey(u => u.Book_Id); 

        modelBuilder.Entity<BookModel>()
            .Property(u => u.Book_Id)
            .ValueGeneratedOnAdd();

        // Borrow Model
        modelBuilder.Entity<BorrowModel>()
            .HasKey(u => u.Borrow_Id); 

        modelBuilder.Entity<BorrowModel>()
            .Property(u => u.Borrow_Id)
            .ValueGeneratedOnAdd();

        // Borrow Book Model
        modelBuilder.Entity<BorrowBookModel>()
            .HasKey(u => u.Borrow_Book_Id); 

        modelBuilder.Entity<BorrowBookModel>()
            .Property(u => u.Borrow_Book_Id)
            .ValueGeneratedOnAdd();

        // Return Book Model
        modelBuilder.Entity<ReturnModel>()
            .HasKey(u => u.Return_Id); 

        modelBuilder.Entity<ReturnModel>()
            .Property(u => u.Return_Id)
            .ValueGeneratedOnAdd();








        // One -> One = User and Student
        modelBuilder.Entity<StudentModel>()
            .HasOne(s => s.User)
            .WithOne(u => u.Student)
            .HasForeignKey<StudentModel>(s => s.User_Id);

        // One -> Many = User and Borrow
        modelBuilder.Entity<BorrowModel>()
            .HasOne(u => u.User)
            .WithMany(b => b.Borrows)
            .HasForeignKey(p => p.User_Id);

        // Many -> Many = Borrow and Book with BorrowBook
        modelBuilder.Entity<BorrowBookModel>()
            .HasOne(bb => bb.Borrow)
            .WithMany(b => b.BorrowBooks)
            .HasForeignKey(bb => bb.Borrow_Id);

        modelBuilder.Entity<BorrowBookModel>()
            .HasOne(bb => bb.Book)
            .WithMany(b => b.BorrowBooks)
            .HasForeignKey(bb => bb.Book_Id);

        // Many -> Many = Borrow and Book with Return
        modelBuilder.Entity<ReturnModel>()
            .HasOne(bb => bb.Borrow)
            .WithMany(b => b.Returns)
            .HasForeignKey(bb => bb.Borrow_Id);

        modelBuilder.Entity<ReturnModel>()
            .HasOne(bb => bb.Book)
            .WithMany(b => b.Returns)
            .HasForeignKey(bb => bb.Book_Id);
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=localhost,1433;Database=rangga_perpusdb;User Id=sa;Password=Rangga#05;TrustServerCertificate=true;");
    }
}


