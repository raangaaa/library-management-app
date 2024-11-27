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

public partial class ReturnPageViewModel : ViewModelBase 
{
    [ObservableProperty]
    private bool _isPaneOpen;

    [RelayCommand]
    private void TriggerPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    [ObservableProperty]
    private int _amount;

    [ObservableProperty]
    private BookModel? _selectedBook;
    [ObservableProperty]
    private SelectedBooks? _selectedBookForDelete;
    public ObservableCollection<SelectedBooks> SelectedBooks { get; } = [];
    public List<ReturnModel> ReturnBooks { get; } = [];
    public List<int> SelectedBookIds { get; } = [];
    [RelayCommand]
    public void AddSelectedBook()
    {
        if (SelectedBook == null)
        {
            Errors.Add("Please select a book first.");
            return;
        }

        if (SelectedBorrow == null)
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
        ReturnBooks.Add(new ReturnModel { Book_Id = SelectedBook!.Book_Id, Return_Date = DateTime.Today, Penalty = SelectedBorrow.Return_Date < DateTime.Today ? (DateTime.Today - SelectedBorrow.Return_Date).Days * 1000 : 0 });
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
            var bookToRemove = ReturnBooks.FirstOrDefault(b => b.Book_Id == bookIdToRemove);

            SelectedBookIds.Remove(bookIdToRemove);
            ReturnBooks.Remove(bookToRemove!);
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
    private BorrowModel? _selectedBorrow;


    public ObservableCollection<BorrowModel> Borrows { get; } = [];
    public ObservableCollection<BookModel> Books { get; } = [];
    public ObservableCollection<string> Errors { get; } = [];

    public IAsyncRelayCommand SaveReturnBookCommand { get; }
    // public IAsyncRelayCommand UpdateBorrowCommand { get; }
    // public IAsyncRelayCommand DeleteBorrowCommand { get; }
    public IAsyncRelayCommand LoadBorrowsCommand { get; }

    public ReturnPageViewModel()
    {
        SaveReturnBookCommand = new AsyncRelayCommand(SaveReturnBook);
        // UpdateBorrowCommand = new AsyncRelayCommand(UpdateBorrow);
        // DeleteBorrowCommand = new AsyncRelayCommand(DeleteBorrow);
        LoadBorrowsCommand = new AsyncRelayCommand(LoadBorrows);

        _ = LoadBorrows();
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
                    .Include(b => b.BorrowBooks!)
                        .ThenInclude(bb => bb.Book)
                    .Include(b => b.Returns!)
                        .ThenInclude(bb => bb.Book)
                    .Include(b => b.User!)
                        .ThenInclude(u => u.Student)
                    .ToListAsync();

                foreach (var borrow in borrows)
                {
                    Borrows.Add(borrow);
                }

                Errors.Clear();
            }
            else
            {
                Console.WriteLine("Database or Return DbSet is null.");
            }
        }
        catch (Exception ex)
        {
            Errors.Clear();
            Errors.Add($"Error loading borrows: {ex.Message}");
        }
    }

    private async Task SaveReturnBook()
    {
        var lifetime = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var window = lifetime?.Windows?.FirstOrDefault(x => x is MainWindow) as MainWindow;

        if (SelectedBorrow == null)
        {
            Errors.Add($"Select data borrow first to make returned data book");
            return;
        }

        try
        {
            using var db = new DatabaseService();
            if (db?.Borrows != null && db?.Books != null)
            {
                var borrow = await db.Borrows
                    .Include(b => b.BorrowBooks!)
                        .ThenInclude(bb => bb.Book)
                    .Include(b => b.Returns)
                    .FirstOrDefaultAsync(b => b.Borrow_Id == SelectedBorrow.Borrow_Id);

                if (borrow != null)
                {
                    if (borrow.BorrowBooks == null)
                    {
                        Errors.Add("Borrow books is null");
                        return;
                    }

                    foreach (var borrowBook in borrow.BorrowBooks)
                    {
                        var returned = ReturnBooks.FirstOrDefault(r => r.Book_Id == borrowBook.Book_Id);

                        if (returned == null)
                        {
                            continue;
                        }
                        if (SelectedBooks == null)
                        {
                            Errors.Add("SelectedBooks books list is null");
                            return;
                        }
                        
                        var select = SelectedBooks.FirstOrDefault(b => b.Book!.Book_Id == returned.Book_Id);
                        var book = await db.Books.FirstOrDefaultAsync(b => b.Book_Id == borrowBook.Book_Id);

                        

                        if (returned != null && borrowBook.Borrow_Amount == 0)
                        {
                            Errors.Add($"Book {book!.Title} was returned");
                            return;
                        }

                        if (returned != null)
                        {
                            if (borrow.Return_Date > DateTime.Today && borrow.Returns!.Count <= 0)
                            {
                                Console.WriteLine("pertama");
                                borrow.Penalty = 0;
                            }
                            if (borrow.Return_Date > DateTime.Today && borrow.Returns!.Count > 0)
                            {
                                Console.WriteLine("kedua");
                                borrow.Penalty = 0;
                            }
                            if (borrow.Return_Date < DateTime.Today && borrow.Returns!.Count <= 0)
                            {
                                Console.WriteLine("ketiga");
                                borrow.Penalty = 0;
                                borrow.Penalty += (DateTime.Today - borrow.Return_Date).Days * 1000;
                            }
                            if (borrow.Return_Date < DateTime.Today && borrow.Returns!.Count > 0)
                            {
                                Console.WriteLine("keempat");
                                borrow.Penalty += (DateTime.Today - borrow.Return_Date).Days * 1000;
                            }

                            if (select!.Amount > borrowBook.Borrow_Amount)
                            {
                                Errors.Add($"Max returned amount is {borrowBook.Borrow_Amount} for book {borrowBook.Book!.Title}");
                                return;
                            }

                            borrowBook.Borrow_Amount -= select!.Amount;
                            book!.Stock += select!.Amount;

                            if (borrow.Returns == null)
                            {
                                borrow.Returns = ReturnBooks;
                            }
                            else
                            {
                                borrow.Returns.Add(returned);
                            }
                        }
                        
                    }

                }

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

    // private async Task UpdateBorrow()
    // {
    //     var lifetime = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
    //     var window = lifetime?.Windows?.FirstOrDefault(x => x is MainWindow) as MainWindow;

    //     if (SelectedBorrow == null)
    //     {
    //         window?.NotificationService.Show("Error", "No borrow data selected for update!");
    //         return;
    //     }

    //     if (string.IsNullOrEmpty(Nis) && SelectedBooks.Count <= 0 && LoanDuration <= 0)
    //     {
    //         Errors.Add("Fill Nis or Duration and select book first");
    //         return;
    //     }

    //     if (BorrowBooks.Count > 1)
    //     {
    //         foreach (var borrowBookMax2 in BorrowBooks)
    //         {
    //             if (borrowBookMax2.Borrow_Amount > 2)
    //             {
    //                 Errors.Add($"Book {borrowBookMax2!.Book!.Title} max request is 2 for all book");
    //             }
    //         }
    //     }


    //     try
    //     {
    //         using var db = new DatabaseService();
    //         if (db?.Students != null && db?.Books != null && db?.Borrows != null && db?.BorrowBooks != null)
    //         {
    //             var student = await db.Students
    //                 .Include(s => s.User)
    //                 .FirstOrDefaultAsync(s => s.NIS == Nis);

    //             var bookExist = await db.Books
    //                 .CountAsync(book => SelectedBookIds.Contains(book.Book_Id)) == SelectedBookIds.Count;

    //             if (bookExist && student == null)
    //             {
    //                 Errors.Clear();
    //                 Errors.Add("Student Nis or Book Not Found:");
    //                 return;
    //             }

    //             // -----------------------------------------------------------

    //             var borrow = await db.Borrows
    //                 .Include(bb => bb.BorrowBooks)
    //                 .FirstOrDefaultAsync(s => s.Borrow_Id == SelectedBorrow.Borrow_Id);
                

    //             if (borrow != null)
    //             {
    //                 foreach (var borrowBook in borrow!.BorrowBooks!)
    //                 {
    //                     var book = await db.Books
    //                         .FirstOrDefaultAsync(b => b.Book_Id == borrowBook.Book_Id);
                        
    //                     if (book != null)
    //                     {
    //                         book.Stock += borrowBook.Borrow_Amount;
    //                     }
    //                 }

    //                 borrow.User_Id = student!.User!.User_Id;
    //                 borrow.Return_Date = borrow.Borrow_Date.AddDays(LoanDuration);
    //                 borrow.BorrowBooks = BorrowBooks;

    //                 foreach (var borrowBook in BorrowBooks)
    //                 {
    //                     var book = await db.Books
    //                                     .FirstOrDefaultAsync(b => b.Book_Id == borrowBook.Book_Id);

    //                     if (book == null)
    //                     {
    //                         Errors.Add($"Book with ID {borrowBook.Book_Id} does not exist.");
    //                         return;
    //                     }

    //                     if (borrowBook.Borrow_Amount > book.Stock)
    //                     {
    //                         Errors.Add($"Insufficient stock for book '{book.Title}'. Available stock: {book.Stock}, Requested: {borrowBook.Borrow_Amount}");
    //                         return;
    //                     }

    //                     if (book != null)
    //                     {
    //                         book.Stock -= borrowBook.Borrow_Amount;
    //                     }
    //                 }

    //                 await db.SaveChangesAsync();
    //                 await LoadReturns();

    //                 ResetFields();
    //                 Errors.Clear();

    //                 window?.NotificationService.Show("Update", "Selected borrow successfully updated!");
    //             }
    //             else
    //             {
    //                 Errors.Add("Borrow not found!");
    //                 return;
    //             }
    //         }
    //         else
    //         {
    //             Console.WriteLine("Database or Borrows DbSet is null.");
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         Errors.Clear();
    //         Errors.Add($"Error loading student and book: {ex.Message}");
    //     }
    // }

    // private async Task DeleteBorrow()
    // {
    //     var lifetime = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
    //     var window = lifetime?.Windows?.FirstOrDefault(x => x is MainWindow) as MainWindow;

    //     if (SelectedBorrow == null)
    //     {
    //         window?.NotificationService.Show("Error", "No borrow selected for delete!");
    //         return;
    //     }

    //     try
    //     {
    //         using var db = new DatabaseService();
    //         if (db.Borrows != null && db.Return != null && db.Books != null)
    //         {
    //             var borrow = await db.Borrows
    //                 .Include(bb => bb.BorrowBooks)
    //                 .FirstOrDefaultAsync(u => u.Borrow_Id == SelectedBorrow.Borrow_Id);


    //             if (borrow != null)
    //             {
    //                 var returnBooks = await db.Return
    //                     .Where(b => b.Borrow_Id == borrow.Borrow_Id)
    //                     .ToListAsync();

    //                 if (returnBooks.Count <= 0)
    //                 {
    //                     foreach (var borrowBook in borrow.BorrowBooks!)
    //                     {
    //                         var book = await db.Books
    //                             .FirstOrDefaultAsync(b => b.Book_Id == borrowBook.Book_Id);

    //                         if (book != null)
    //                         {
    //                             book.Stock += borrowBook.Borrow_Amount;
    //                         }
    //                     }
    //                 }

    //                 if (returnBooks.Count > 0)
    //                 {
    //                     window?.NotificationService.Show("Error", "Can't delete this borrow, student are was return some borrowed book!");
    //                     return;
    //                 }
                    
    //                 db.Borrows.Remove(borrow);
    //                 await db.SaveChangesAsync();
    //                 Borrows.Remove(SelectedBorrow);

    //                 ResetFields();
    //                 Errors.Clear();

    //                 window?.NotificationService.Show("Delete", "Selected borrow was deleted!");
    //             } 
    //             else
    //             {
    //                 window?.NotificationService.Show("Error", "Borrow data not found!");
    //             }
    //         }
    //         else
    //         {
    //             Console.WriteLine("Database or Students DbSet is null.");
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         Errors.Clear();
    //         Errors.Add($"Error deleting student: {ex.Message}");
    //     }
    // }


    
    private void LoadBooks()
    {
        if (SelectedBorrow == null)
        {
            Errors.Add("Select the borrow");
            return;
        }

        try
        {
            using var db = new DatabaseService();
            if (db?.Books != null && db?.Borrows != null)
            {
                foreach (var borrowBook in SelectedBorrow!.BorrowBooks!)
                {
                    if (borrowBook.Book == null)
                    {
                        Errors.Add("Book not found");
                        return;
                    }
                    Books.Add(borrowBook.Book);
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

        LoadBooks();

        Errors.Clear();
    }


    [RelayCommand]
    public void ResetFields()
    {
        SelectedBook = null;
        SelectedBookForDelete = null;
        SelectedBooks.Clear();
        ReturnBooks.Clear();
        SelectedBookIds.Clear();
        SelectedBorrow = null;
        Books.Clear();
        Errors.Clear();
    }
}