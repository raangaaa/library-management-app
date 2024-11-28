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
    private string? _search;

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


        if (SelectedBookIds.Contains(SelectedBook.Book_Id))
        {
            Errors.Add("This book is already added.");
            return;
        }

        SelectedBooks.Add(new SelectedBooks { Book = SelectedBook });
        ReturnBooks.Add(new ReturnModel { Book_Id = SelectedBook!.Book_Id, Return_Date = DateTime.Now, Penalty = SelectedBorrow.Return_Date < DateTime.Today ? (DateTime.Today - SelectedBorrow.Return_Date).Days * 1000 : 0 });
        SelectedBookIds.Add(SelectedBook.Book_Id);
        SelectedBook = null;
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
    public IAsyncRelayCommand UpdateReturnBookCommand { get; }
    public IAsyncRelayCommand DeleteReturnBookCommand { get; }
    public IAsyncRelayCommand LoadBorrowsCommand { get; }
    public IAsyncRelayCommand SearchBorrowsCommand { get; }


    public ReturnPageViewModel()
    {
        SaveReturnBookCommand = new AsyncRelayCommand(SaveReturnBook);
        UpdateReturnBookCommand = new AsyncRelayCommand(UpdateReturnBook);
        DeleteReturnBookCommand = new AsyncRelayCommand(DeleteReturnBook);
        LoadBorrowsCommand = new AsyncRelayCommand(LoadBorrows);
        SearchBorrowsCommand = new AsyncRelayCommand(SearchBorrows);

        _ = LoadBorrows();
    }

    private async Task SearchBorrows()
    {
        var lifetime = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var window = lifetime?.Windows?.FirstOrDefault(x => x is MainWindow) as MainWindow;
        if(string.IsNullOrEmpty(Search))
        {
            Errors.Clear();
            Errors.Add("Fill search");
            return;
        }
        try
        {
            using var db = new DatabaseService();
            if (db?.Books != null && db?.Students != null && db?.Borrows != null)
            {
                Borrows.Clear();
                var student = await db.Students
                    .FirstOrDefaultAsync(b => EF.Functions.Like(b.NIS, $"%{Search}%"));
                
                if (student == null)
                {
                    window?.NotificationService.Show("Not found", $"Student with Nis {Search} not found!");
                }

                var borrows = await db.Borrows
                    .Include(b => b.BorrowBooks!)
                        .ThenInclude(bb => bb.Book)
                    .Include(b => b.User!)
                        .ThenInclude(u => u.Student)
                    .Include(b => b.Returns!)
                        .ThenInclude(bb => bb.Book)
                    .Where(b => b.User_Id == student!.User_Id)
                    .ToListAsync();
                    
                foreach (var borrow in borrows)
                {
                    Borrows.Add(borrow);
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
                        
                        var book = await db.Books.FirstOrDefaultAsync(b => b.Book_Id == borrowBook.Book_Id);


                        if (returned != null)
                        {
                            // tidak terlambat dan pertama kali mengembalikan
                            if (borrow.Return_Date > DateTime.Today && borrow.Returns!.Count <= 0)
                            {
                                borrow.Penalty = 0;
                            }
                            // tidak terlambat dan sudah pernah mengembalikan
                            if (borrow.Return_Date > DateTime.Today && borrow.Returns!.Count > 0)
                            {
                                borrow.Penalty = 0;
                            }
                            // terlambat dan pertama kali mengembalikan
                            if (borrow.Return_Date < DateTime.Today && borrow.Returns!.Count <= 0)
                            {
                                borrow.Penalty = 0;
                                borrow.Penalty += (DateTime.Today - borrow.Return_Date).Days * 1000;
                            }
                            // terlambat dan sudah pernah mengembalikan
                            if (borrow.Return_Date < DateTime.Today && borrow.Returns!.Count > 0)
                            {
                                borrow.Penalty += (DateTime.Today - borrow.Return_Date).Days * 1000;
                            }

                            if (borrow.Returns!.Any(r => r.Book_Id == borrowBook.Book_Id))
                            {
                                Errors.Add($"Book {borrowBook.Book!.Title} was returned");
                                return;
                            }

                            book!.Stock += borrowBook.Borrow_Amount;

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

    private async Task UpdateReturnBook()
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

                // -----------------------------------------------------------

                var borrow = await db.Borrows
                    .Include(b => b.BorrowBooks!)
                        .ThenInclude(bb => bb.Book)
                    .Include(b => b.Returns)
                    .FirstOrDefaultAsync(s => s.Borrow_Id == SelectedBorrow.Borrow_Id);
                

                if (borrow != null)
                {
                    if (borrow.BorrowBooks == null)
                    {
                        Errors.Add("Borrow books is null");
                        return;
                    }

                    if (borrow.Returns == null)
                    {
                        Errors.Add("Return books is null");
                        return;
                    }

                    foreach (var returnedBook in borrow.Returns)
                    {
                        borrow.Penalty -= returnedBook.Penalty;
                    }

                    foreach (var borrowBook in borrow.BorrowBooks)
                    {
                        if (borrowBook.Book == null)
                        {
                            Errors.Add("Borrow book is null");
                            return;
                        }

                        borrowBook.Book.Stock -= borrowBook.Borrow_Amount;
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
                        
                        var book = await db.Books.FirstOrDefaultAsync(b => b.Book_Id == borrowBook.Book_Id);


                        if (returned != null)
                        {
                            // tidak terlambat dan pertama kali mengembalikan
                            if (borrow.Return_Date > DateTime.Today && borrow.Returns!.Count <= 0)
                            {
                                borrow.Penalty = 0;
                            }
                            // tidak terlambat dan sudah pernah mengembalikan
                            if (borrow.Return_Date > DateTime.Today && borrow.Returns!.Count > 0)
                            {
                                borrow.Penalty = 0;
                            }
                            // terlambat dan pertama kali mengembalikan
                            if (borrow.Return_Date < DateTime.Today && borrow.Returns!.Count <= 0)
                            {
                                borrow.Penalty = 0;
                                borrow.Penalty += (DateTime.Today - borrow.Return_Date).Days * 1000;
                            }
                            // terlambat dan sudah pernah mengembalikan
                            if (borrow.Return_Date < DateTime.Today && borrow.Returns!.Count > 0)
                            {
                                borrow.Penalty += (DateTime.Today - borrow.Return_Date).Days * 1000;
                            }

                            borrow.Returns.Clear();

                            if (borrow.Returns!.Any(r => r.Book_Id == borrowBook.Book_Id))
                            {
                                Errors.Add($"Book {borrowBook.Book!.Title} was returned");
                                return;
                            }


                            book!.Stock += borrowBook.Borrow_Amount;

                            if (borrow.Returns == null)
                            {
                                borrow.Returns = ReturnBooks;
                            }
                            else
                            {
                                borrow.Returns.Add(returned);
                            }
                            
                        }
                        else 
                        {
                            Errors.Add("returned books list is null");
                            return;
                        }
                        
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
                Console.WriteLine("Database or Borrows DbSet is null.");
            }
        }
        catch (Exception ex)
        {
            Errors.Clear();
            Errors.Add($"Error loading: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    private async Task DeleteReturnBook()
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
            if (db.Borrows != null && db.Books != null)
            {
                var borrow = await db.Borrows
                    .Include(b => b.BorrowBooks!)
                    .Include(b => b.Returns!)
                    .FirstOrDefaultAsync(b => b.Borrow_Id == SelectedBorrow!.Borrow_Id);

                foreach (var borrowBook in borrow!.BorrowBooks!)
                {
                    if (borrow.Returns == null)
                    {
                        break;
                    }
                    var returned = borrow.Returns.FirstOrDefault(r => r.Book_Id == borrowBook.Book_Id);
                    var book = await db.Books.FirstOrDefaultAsync(b => b.Book_Id == borrowBook.Book_Id);

                    book!.Stock -= borrowBook.Borrow_Amount;
                    borrow.Penalty -= returned!.Penalty;
                }

                borrow!.Returns!.Clear();
                await db.SaveChangesAsync();

                ResetFields();
                Errors.Clear();
                await LoadBorrows();

                window?.NotificationService.Show("Delete", "Selected all returned book was deleted!");
               
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
            Console.WriteLine(ex.StackTrace);
        }
    }


    
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

                foreach (var returnedBook in SelectedBorrow!.Returns!)
                {
                    if (returnedBook.Book == null)
                    {
                        Errors.Add("Book not found");
                        return;
                    }
                    SelectedBooks.Add(new SelectedBooks { Book = returnedBook.Book });
                    ReturnBooks.Add(new ReturnModel { Book_Id = returnedBook.Book_Id, Return_Date = DateTime.Now, Penalty = SelectedBorrow.Return_Date < DateTime.Today ? (DateTime.Today - SelectedBorrow.Return_Date).Days * 1000 : 0 });
                    SelectedBookIds.Add(returnedBook.Book_Id);
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