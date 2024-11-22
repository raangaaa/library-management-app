using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Perpustakaan.Models;
using Perpustakaan.Services;
using Perpustakaan.Views;

namespace Perpustakaan.ViewModels;

public partial class BorrowPageViewModel : ViewModelBase 
{
    [ObservableProperty]
    private bool _isPaneOpen;

    [RelayCommand]
    private void TriggerPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }


    [ObservableProperty]
    private string? _nis;
    [ObservableProperty]
    private int _bookId;
    [ObservableProperty]
    private int _amount;

    [ObservableProperty]
    private BorrowModel? _selectedBorrow;
    public ObservableCollection<BorrowModel> Borrows { get; } = [];
    public ObservableCollection<BookModel> Books { get; } = [];
    public ObservableCollection<UserModel> Students { get; } = [];
    public ObservableCollection<string> Errors { get; } = [];

    public IAsyncRelayCommand SaveBorrowCommand { get; }
    public IAsyncRelayCommand UpdateBorrowCommand { get; }
    public IAsyncRelayCommand DeleteBorrowCommand { get; }
    public IAsyncRelayCommand LoadBorrowsCommand { get; }

    public BorrowPageViewModel()
    {
        SaveBorrowCommand = new AsyncRelayCommand(SaveBorrow);
        UpdateBorrowCommand = new AsyncRelayCommand(UpdateBorrow);
        DeleteBorrowCommand = new AsyncRelayCommand(DeleteBorrow);
        LoadBorrowsCommand = new AsyncRelayCommand(LoadBorrows);

        _ = LoadBorrows();
        _ = LoadBooks();
        _ = LoadStudents();
    }

    private async Task LoadBorrows()
    {
        try
        {
            using var db = new DatabaseService();
            if (db?.Borrows != null)
            {
                Borrows.Clear();
                var borrows = await db.Borrows
                        .Include(b => b.User!)
                            .ThenInclude(u => u.Student)
                        .Include(b => b.BorrowBooks!)
                            .ThenInclude(bb => bb.Book)
                        .ToListAsync();

                foreach (var borrow in borrows)
                {
                    Borrows.Add(borrow);
                }
            }
            else
            {
                Console.WriteLine("Database or Borrows DbSet is null.");
            }
        }
        catch (Exception ex)
        {
            Errors.Clear();
            Errors.Add($"Error loading borrows: {ex.Message}");
        }
    }

    private async Task SaveBorrow()
    {
        var lifetime = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var window = lifetime?.Windows?.FirstOrDefault(x => x is MainWindow) as MainWindow;

        var today = DateTime.Today;
        var nextWeek = DateTime.Today.AddDays(7);
        TimeSpan difference = nextWeek - today; 

        try
        {
            using var db = new DatabaseService();
            if (db?.Students != null && db?.Books != null && db?.Borrows != null && db?.BorrowBooks != null)
            {
                var student = await db.Students
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.NIS == Nis);

                var book = await db.Books
                    .FirstOrDefaultAsync(s => s.Book_Id == BookId);

                if (book == null && student == null)
                {
                    Errors.Clear();
                    Errors.Add("Student Nis or Book Id Not Found:");
                    return;
                }

                // -----------------------------------------------------------

                var borrow = new BorrowModel
                {
                    User_Id = student!.User!.User_Id,
                    Borrow_Date = today,
                    Return_Date = nextWeek,
                    Loan_Duration = difference.Days,
                    Penalty = 0
                };

                var validatorBorrow = new BorrowModelValidator();
                var results = validatorBorrow.Validate(borrow);

                if (!results.IsValid)
                {
                    Errors.Clear();
                    foreach (var failure in results.Errors)
                    {
                        Errors.Add($"Column {failure.PropertyName} failed validation. Error: {failure.ErrorMessage}");
                    }
                    return;
                }

                // ------------------------------------------------------------

                var borrowAfter = await db.Borrows.AddAsync(borrow);
                await db.SaveChangesAsync();
                var borrowBook = new BorrowBookModel
                {
                    Borrow_Id = borrowAfter.Entity.Borrow_Id,
                    Book_Id = BookId,
                    Borrow_Amount = Amount,
                };
                await db.BorrowBooks.AddAsync(borrowBook);
                await db.SaveChangesAsync();
                Borrows.Add(borrow);

                ResetFields();

                window?.NotificationService.Show("Success", "Borrow successfully created!");
            }
            else
            {
                Console.WriteLine("Database or Borrows DbSet is null.");
            }
        }
        catch (Exception ex)
        {
            Errors.Clear();
            Errors.Add($"Error loading student and book: {ex.Message}");
        }

    }

    private async Task UpdateBorrow()
    {
        var lifetime = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var window = lifetime?.Windows?.FirstOrDefault(x => x is MainWindow) as MainWindow;

        if (SelectedBorrow == null)
        {
            window?.NotificationService.Show("Error", "No student selected for update!");
            return;
        }

        try
        {
            using var db = new DatabaseService();
            if (db?.Students != null && db?.Books != null && db?.Borrows != null && db?.BorrowBooks != null)
            {
                var student = await db.Students
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.NIS == Nis);

                var book = await db.Books
                    .FirstOrDefaultAsync(s => s.Book_Id == BookId);

                if (book == null && student == null)
                {
                    Errors.Clear();
                    Errors.Add("Student Nis or Book Id Not Found:");
                    return;
                }

                // -----------------------------------------------------------

                var borrow = await db.Borrows
                    .Include(b => b.User!)
                        .ThenInclude(u => u.Student)
                    .Include(b => b.BorrowBooks!)
                        .ThenInclude(bb => bb.Book)
                    .FirstOrDefaultAsync(s => s.Borrow_Id == SelectedBorrow.Borrow_Id);

                var borrowBooks = await db.BorrowBooks
                    .FirstOrDefaultAsync(s => s.Borrow_Id == SelectedBorrow.Borrow_Id);

                if (borrow != null && borrowBooks != null)
                {
                    borrow.User_Id = student!.User!.User_Id;   
                    borrowBooks.Book_Id = book!.Book_Id;
                    borrowBooks.Borrow_Amount = Amount;

                    await db.SaveChangesAsync();
                    await LoadBorrows();

                    window?.NotificationService.Show("Update", "Selected borrow successfully updated!");
                }
                else
                {
                    window?.NotificationService.Show("Error", "Borrow not found!");
                }
            }
            else
            {
                Console.WriteLine("Database or Borrows DbSet is null.");
            }
        }
        catch (Exception ex)
        {
            Errors.Clear();
            Errors.Add($"Error loading student and book: {ex.Message}");
        }
    }

    private async Task DeleteBorrow()
    {
        var lifetime = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var window = lifetime?.Windows?.FirstOrDefault(x => x is MainWindow) as MainWindow;

        if (SelectedBorrow == null)
        {
            window?.NotificationService.Show("Error", "No borrow selected for delete!");
            return;
        }

        try
        {
            using var db = new DatabaseService();
            if (db.Borrows != null)
            {
                var borrow = await db.Borrows
                    .Include(b => b.BorrowBooks)
                    .FirstOrDefaultAsync(u => u.Borrow_Id == SelectedBorrow.Borrow_Id);

                if (borrow != null)
                {
                    db.Borrows.Remove(borrow);
                    await db.SaveChangesAsync();
                    Borrows.Remove(SelectedBorrow);
                    ResetFields();

                    window?.NotificationService.Show("Delete", "Selected borrow was deleted!");
                } 
                else
                {
                    window?.NotificationService.Show("Error", "Student not found!");
                }
            }
            else
            {
                Console.WriteLine("Database or Students DbSet is null.");
            }
        }
        catch (Exception ex)
        {
            Errors.Clear();
            Errors.Add($"Error deleting student: {ex.Message}");
        }
    }


    
    private async Task LoadBooks()
    {
        try
        {
            using var db = new DatabaseService();
            if (db?.Books != null)
            {
                Books.Clear();
                var books = await db.Books.ToListAsync();

                foreach (var book in books)
                {
                    Books.Add(book);
                }
            }
            else
            {
                Console.WriteLine("Database or Books DbSet is null.");
            }
        }
        catch (Exception ex)
        {
            Errors.Clear();
            Errors.Add($"Error loading books: {ex.Message}");
        }
    }

    private async Task LoadStudents()
    {
        try
        {
            using var db = new DatabaseService();
            if (db?.Users != null)
            {
                Students.Clear();
                var students = await db.Users
                        .Include(u => u.Student)
                        .ToListAsync();

                foreach (var student in students)
                {
                    Students.Add(student);
                }
            }
            else
            {
                Console.WriteLine("Database or Students DbSet is null.");
            }
        }
        catch (Exception ex)
        {
            Errors.Clear();
            Errors.Add($"Error loading students: {ex.Message}");
        }
    }




    private void ResetFields()
    {
        Nis = string.Empty;
        BookId = 0;
        Amount = 0;
        // SelectedStudent = null;
    }
}