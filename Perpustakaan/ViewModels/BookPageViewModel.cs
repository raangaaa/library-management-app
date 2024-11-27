using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Perpustakaan.Models;
using Perpustakaan.Services;
using Perpustakaan.Views;
using Avalonia.Controls.ApplicationLifetimes;

namespace Perpustakaan.ViewModels;

public partial class BookPageViewModel : ViewModelBase 
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
    private string? _title;
    [ObservableProperty]
    private string? _author;
    [ObservableProperty]
    private string? _publisher;
    [ObservableProperty]
    private int _year;
    [ObservableProperty]
    private int _stock;

    [ObservableProperty]
    private BookModel? _selectedBook;
    public ObservableCollection<BookModel> Books { get; } = [];
    public ObservableCollection<string> Errors { get; } = [];

    public IAsyncRelayCommand SaveBookCommand { get; }
    public IAsyncRelayCommand UpdateBookCommand { get; }
    public IAsyncRelayCommand DeleteBookCommand { get; }
    public IAsyncRelayCommand LoadBooksCommand { get; }
    public IAsyncRelayCommand SearchBooksCommand { get; }

    public BookPageViewModel()
    {
        SaveBookCommand = new AsyncRelayCommand(SaveBook);
        UpdateBookCommand = new AsyncRelayCommand(UpdateBook);
        DeleteBookCommand = new AsyncRelayCommand(DeleteBook);
        LoadBooksCommand = new AsyncRelayCommand(LoadBooks);
        SearchBooksCommand = new AsyncRelayCommand(SearchBooks);

        _ = LoadBooks();
    }

    private async Task SearchBooks()
    {
        if(string.IsNullOrEmpty(Search))
        {
            Errors.Clear();
            Errors.Add("Fill search");
            return;
        }
        try
        {
            using var db = new DatabaseService();
            if (db?.Books != null)
            {
                Books.Clear();
                var books = await db.Books
                    .Where(b => EF.Functions.Like(b.Title, $"%{Search}%"))
                    .ToListAsync();

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

    private async Task SaveBook()
    {
        var lifetime = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var window = lifetime?.Windows?.FirstOrDefault(x => x is MainWindow) as MainWindow;

        var book = new BookModel
        {
            Title = Title,
            Author = Author,
            Publisher = Publisher,
            Year = Year,
            Stock = Stock
        };

        var validatorBook = new BookModelValidator();
        var results = validatorBook.Validate(book);

        if (!results.IsValid)
        {
            Errors.Clear();
            foreach (var failure in results.Errors)
            {
                Errors.Add($"Column {failure.PropertyName} failed validation. Error: {failure.ErrorMessage}");
            }
            return;
        }

        try
        {
            using var db = new DatabaseService();
            if (db?.Books != null)
            {
                await db.Books.AddAsync(book);
                await db.SaveChangesAsync();
                Books.Add(book);

                ResetFields();

                window?.NotificationService.Show("Success", "New book successfully added!");
            }
            else
            {
                Console.WriteLine("Database or Books DbSet is null.");
            }
        }
        catch (Exception ex)
        {
            Errors.Clear();
            Errors.Add($"Error: {ex.Message}");
        }
    }

    private async Task UpdateBook()
    {
        var lifetime = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var window = lifetime?.Windows?.FirstOrDefault(x => x is MainWindow) as MainWindow;

        if (SelectedBook == null)
        {
            window?.NotificationService.Show("Error", "No book selected for update!");
            return;
        }

        var editBook = new BookModel
        {
            Title = Title,
            Author = Author,
            Publisher = Publisher,
            Year = Year,
            Stock = Stock
        };

        var validatorBook = new BookModelValidator();
        var results = validatorBook.Validate(editBook);

        if (!results.IsValid)
        {
            Errors.Clear();
            foreach (var failure in results.Errors)
            {
                Errors.Add($"Column {failure.PropertyName} failed validation. Error: {failure.ErrorMessage}");
            }
            return;
        }

        try
        {
            using var db = new DatabaseService();
            if (db?.Books != null)
            {
                var book = await db.Books.FindAsync(SelectedBook.Book_Id);
                if (book != null)
                {
                    book.Title = Title;
                    book.Author = Author;
                    book.Publisher = Publisher;
                    book.Year = Year;
                    book.Stock = Stock;

                    await db.SaveChangesAsync();
                    await LoadBooks();

                    window?.NotificationService.Show("Update", "Selected book successfully updated!");
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
            Errors.Add($"Error updating book: {ex.Message}");
        }
    }

    private async Task DeleteBook()
    {
        var lifetime = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var window = lifetime?.Windows?.FirstOrDefault(x => x is MainWindow) as MainWindow;
        
        if (SelectedBook == null)
        {
            window?.NotificationService.Show("Error", "No book selected for delete!");
            return;
        }

        try
        {
            using var db = new DatabaseService();
            if (db?.Books != null)
            {
                db.Books.Remove(SelectedBook);
                await db.SaveChangesAsync();
                Books.Remove(SelectedBook);
                ResetFields();

                window?.NotificationService.Show("Delete", "Selected book deleted!");
            }
            else
            {
                Console.WriteLine("Database or Books DbSet is null.");
            }
        }
        catch (Exception ex)
        {
            Errors.Clear();
            Errors.Add($"Error deleting book: {ex.Message}");
        }
    }



    partial void OnSelectedBookChanged(BookModel? value)
    {
        EditFields();
    }

    private void EditFields()
    {
        Title = SelectedBook?.Title;
        Author = SelectedBook?.Author;
        Publisher = SelectedBook?.Publisher;
        Year = SelectedBook?.Year ?? 0;
        Stock = SelectedBook?.Stock ?? 0;
    }

    [RelayCommand]
    public void ResetFields()
    {
        Title = string.Empty;
        Author = string.Empty;
        Publisher = string.Empty;
        Year = 0;
        Stock = 0;
        SelectedBook = null;
    }
}