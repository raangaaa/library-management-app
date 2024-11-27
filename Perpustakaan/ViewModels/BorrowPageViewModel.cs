using System;
using System.Collections.Generic;
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
    private BookModel? _selectedBook;
    [ObservableProperty]
    private SelectedBooks? _selectedBookForDelete;
    public ObservableCollection<SelectedBooks> SelectedBooks { get; } = [];
    public List<BorrowBookModel> BorrowBooks { get; } = [];
    public List<int> SelectedBookIds { get; } = [];
    [RelayCommand]
    public void AddSelectedBook()
    {
        if (SelectedBook == null)
        {
            Errors.Add("Please select a book first.");
            return;
        }

        if (Amount <= 0)
        {
            Errors.Add("Please enter a valid amount.");
            return;
        }

        if (Amount > 2)
        {
            Errors.Add("Max amount is 2.");
            return;
        }

        if (SelectedBookIds.Contains(SelectedBook.Book_Id))
        {
            Errors.Add("This book is already added.");
            return;
        }

        SelectedBooks.Add(new SelectedBooks { Book = SelectedBook, Amount = Amount});
        BorrowBooks.Add(new BorrowBookModel { Book_Id = SelectedBook!.Book_Id, Borrow_Amount = Amount});
        SelectedBookIds.Add(SelectedBook.Book_Id);
        SelectedBook = null;
        Amount = 0;
        Errors.Clear();
    }
    [RelayCommand]
    public void DeleteSelectedBook()
    {
        if (SelectedBookForDelete == null)
        {
            Errors.Add("Please select a book to remove.");
            return;
        }

        if (SelectedBookForDelete.Book != null)
        {
            int bookIdToRemove = SelectedBookForDelete.Book.Book_Id;
            var bookToRemove = BorrowBooks.FirstOrDefault(b => b.Book_Id == bookIdToRemove);

            SelectedBookIds.Remove(bookIdToRemove);
            BorrowBooks.Remove(bookToRemove!);
            SelectedBooks.Remove(SelectedBookForDelete);
        }
        else
        {
            Console.WriteLine("SelectedBookForDelete or its Book is null.");
        }

        SelectedBookForDelete = null;
        Errors.Clear();
    }
    [ObservableProperty]
    private int _amount;
    [ObservableProperty]
    private int _loanDuration;

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

                Errors.Clear();
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
        var returnDate = today.AddDays(LoanDuration);

        if (string.IsNullOrEmpty(Nis) && SelectedBooks.Count <= 0 && LoanDuration <= 0)
        {
            Errors.Add("Fill Nis or Duration and select book first");
            return;
        }

        if (BorrowBooks.Count > 1)
        {
            foreach (var borrowBookMax2 in BorrowBooks)
            {
                if (borrowBookMax2.Borrow_Amount > 2)
                {
                    Errors.Add($"Book max request is 2 if you borrow grater than 2 book");
                    return;
                }
            }
        }

        try
        {
            using var db = new DatabaseService();
            if (db?.Students != null && db?.Books != null && db?.Borrows != null)
            {
                var student = await db.Students
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.NIS == Nis);

                var bookExist = await db.Books
                    .CountAsync(book => SelectedBookIds.Contains(book.Book_Id)) == SelectedBookIds.Count;

                if (bookExist && student == null)
                {
                    Errors.Clear();
                    Errors.Add("Student Nis or Book Not Found:");
                    return;
                }

                // -----------------------------------------------------------

                int totalPenalty = 0;

                if (LoanDuration > 7)
                {
                    totalPenalty += (LoanDuration - 7) * 1000;
                }

                var borrow = new BorrowModel
                {
                    User_Id = student!.User!.User_Id,
                    Borrow_Date = today,
                    Return_Date = returnDate,
                    Loan_Duration = LoanDuration,
                    Penalty = totalPenalty,
                    BorrowBooks = BorrowBooks
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

                foreach (var borrowBook in borrow.BorrowBooks)
                {
                    var book = await db.Books
                        .FirstOrDefaultAsync(b => b.Book_Id == borrowBook.Book_Id);

                    if (book == null)
                    {
                        Errors.Add($"Book with ID {borrowBook.Book_Id} does not exist.");
                        return;
                    }

                    if (borrowBook.Borrow_Amount > book.Stock)
                    {
                        Errors.Add($"Insufficient stock for book '{book.Title}'. Available stock: {book.Stock}, Requested: {borrowBook.Borrow_Amount}");
                        return;
                    }

                    if (book != null)
                    {
                        book.Stock -= borrowBook.Borrow_Amount;
                    }
                }

                // ------------------------------------------------------------

                await db.Borrows.AddAsync(borrow);
                await db.SaveChangesAsync();
                await LoadBorrows();

                ResetFields();
                Errors.Clear();

                window?.NotificationService.Show("Success", "Borrow successfully created!");
            }
            else
            {
                Console.WriteLine("Database or DbSet is null.");
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
            window?.NotificationService.Show("Error", "No borrow data selected for update!");
            return;
        }

        if (string.IsNullOrEmpty(Nis) && SelectedBooks.Count <= 0 && LoanDuration <= 0)
        {
            Errors.Add("Fill Nis or Duration and select book first");
            return;
        }

        if (BorrowBooks.Count > 1)
        {
            foreach (var borrowBookMax2 in BorrowBooks)
            {
                if (borrowBookMax2.Borrow_Amount > 2)
                {
                    Errors.Add($"Book {borrowBookMax2!.Book!.Title} max request is 2 for all book");
                }
            }
        }


        try
        {
            using var db = new DatabaseService();
            if (db?.Students != null && db?.Books != null && db?.Borrows != null && db?.BorrowBooks != null)
            {
                var student = await db.Students
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.NIS == Nis);

                var bookExist = await db.Books
                    .CountAsync(book => SelectedBookIds.Contains(book.Book_Id)) == SelectedBookIds.Count;

                if (bookExist && student == null)
                {
                    Errors.Clear();
                    Errors.Add("Student Nis or Book Not Found:");
                    return;
                }

                // -----------------------------------------------------------

                var borrow = await db.Borrows
                    .Include(bb => bb.BorrowBooks)
                    .FirstOrDefaultAsync(s => s.Borrow_Id == SelectedBorrow.Borrow_Id);
                

                if (borrow != null)
                {
                    foreach (var borrowBook in borrow!.BorrowBooks!)
                    {
                        var book = await db.Books
                            .FirstOrDefaultAsync(b => b.Book_Id == borrowBook.Book_Id);
                        
                        if (book != null)
                        {
                            book.Stock += borrowBook.Borrow_Amount;
                        }
                    }

                    borrow.User_Id = student!.User!.User_Id;
                    borrow.Return_Date = borrow.Borrow_Date.AddDays(LoanDuration);
                    borrow.BorrowBooks = BorrowBooks;

                    foreach (var borrowBook in BorrowBooks)
                    {
                        var book = await db.Books
                                        .FirstOrDefaultAsync(b => b.Book_Id == borrowBook.Book_Id);

                        if (book == null)
                        {
                            Errors.Add($"Book with ID {borrowBook.Book_Id} does not exist.");
                            return;
                        }

                        if (borrowBook.Borrow_Amount > book.Stock)
                        {
                            Errors.Add($"Insufficient stock for book '{book.Title}'. Available stock: {book.Stock}, Requested: {borrowBook.Borrow_Amount}");
                            return;
                        }

                        if (book != null)
                        {
                            book.Stock -= borrowBook.Borrow_Amount;
                        }
                    }

                    await db.SaveChangesAsync();
                    await LoadBorrows();

                    ResetFields();
                    Errors.Clear();

                    window?.NotificationService.Show("Update", "Selected borrow successfully updated!");
                }
                else
                {
                    Errors.Add("Borrow not found!");
                    return;
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
            if (db.Borrows != null && db.Return != null && db.Books != null)
            {
                var borrow = await db.Borrows
                    .Include(bb => bb.BorrowBooks)
                    .FirstOrDefaultAsync(u => u.Borrow_Id == SelectedBorrow.Borrow_Id);


                if (borrow != null)
                {
                    var returnBooks = await db.Return
                        .Where(b => b.Borrow_Id == borrow.Borrow_Id)
                        .ToListAsync();

                    if (returnBooks.Count <= 0)
                    {
                        foreach (var borrowBook in borrow.BorrowBooks!)
                        {
                            var book = await db.Books
                                .FirstOrDefaultAsync(b => b.Book_Id == borrowBook.Book_Id);

                            if (book != null)
                            {
                                book.Stock += borrowBook.Borrow_Amount;
                            }
                        }
                    }

                    if (returnBooks.Count > 0)
                    {
                        window?.NotificationService.Show("Error", "Can't delete this borrow, student are was return some borrowed book!");
                        return;
                    }
                    
                    db.Borrows.Remove(borrow);
                    await db.SaveChangesAsync();
                    Borrows.Remove(SelectedBorrow);

                    ResetFields();
                    Errors.Clear();

                    window?.NotificationService.Show("Delete", "Selected borrow was deleted!");
                } 
                else
                {
                    window?.NotificationService.Show("Error", "Borrow data not found!");
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



    partial void OnSelectedBorrowChanged(BorrowModel? value)
    {
        EditFields();
    }

    private void EditFields()
    {
        if (SelectedBorrow == null)
        {
            Errors.Add("SelectedBorrow is null.");
            return;
        }

        if (SelectedBorrow.User == null || SelectedBorrow.User.Student == null)
        {
            Errors.Add("User or Student is null.");
            return;
        }

        Nis = SelectedBorrow.User.Student.NIS;
        LoanDuration = SelectedBorrow.Loan_Duration;
        SelectedBook = null;
        Amount = 0;

        if (SelectedBorrow.BorrowBooks == null)
        {
            Errors.Add("BorrowBooks is null.");
            return;
        }

        foreach (var borrowBook in SelectedBorrow.BorrowBooks!)
        {
            SelectedBooks.Add(new SelectedBooks { Book = borrowBook.Book, Amount = borrowBook.Borrow_Amount });
            BorrowBooks.Add(new BorrowBookModel { Book_Id = borrowBook!.Book_Id, Borrow_Amount = Amount });
            SelectedBookIds.Add(borrowBook.Book_Id);
        }

        Errors.Clear();
    }


    [RelayCommand]
    public void ResetFields()
    {
        Nis = string.Empty;
        SelectedBook = null;
        SelectedBookForDelete = null;
        SelectedBooks.Clear();
        BorrowBooks.Clear();
        SelectedBookIds.Clear();
        Amount = 0;
        LoanDuration = 0;
        SelectedBorrow = null;
        Errors.Clear();
    }
}


public class SelectedBooks
{
    public BookModel? Book { get; set; }
    public int Amount { get; set; }
}