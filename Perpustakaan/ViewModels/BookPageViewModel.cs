using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Perpustakaan.Models;
using Perpustakaan.Services;

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

    public IAsyncRelayCommand SaveBookCommand { get; }
    public IAsyncRelayCommand UpdateBookCommand { get; }
    public IAsyncRelayCommand DeleteBookCommand { get; }
    public IAsyncRelayCommand LoadBooksCommand { get; }

    public BookPageViewModel()
    {
        SaveBookCommand = new AsyncRelayCommand(SaveBook);
        UpdateBookCommand = new AsyncRelayCommand(UpdateBook);
        DeleteBookCommand = new AsyncRelayCommand(DeleteBook);
        LoadBooksCommand = new AsyncRelayCommand(LoadBooks);

        _ = LoadBooks();
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
            Console.WriteLine($"Error loading books: {ex.Message}");
        }
    }

    private async Task SaveBook()
    {
        if (string.IsNullOrWhiteSpace(Title) || Year <= 0 || Stock < 0)
        {
            Console.WriteLine("Invalid input");
            return;
        }

        var book = new BookModel
        {
            Title = Title,
            Author = Author,
            Publisher = Publisher,
            Year = Year,
            Stock = Stock
        };



        try
        {
            using var db = new DatabaseService();
            if (db?.Books != null)
            {
                await db.Books.AddAsync(book);
                await db.SaveChangesAsync();
                Books.Add(book);

                ResetFields();
            }
            else
            {
                Console.WriteLine("Database or Books DbSet is null.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private async Task UpdateBook()
    {
        if (SelectedBook == null)
        {
            Console.WriteLine("No book selected for update.");
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
                    Console.WriteLine("Book updated successfully!");
                }
            }
            else
            {
                Console.WriteLine("Database or Books DbSet is null.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating book: {ex.Message}");
        }
    }

    private async Task DeleteBook()
    {
        if (SelectedBook == null)
        {
            Console.WriteLine("No book selected for deletion.");
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
                Console.WriteLine("Book deleted successfully!");
            }
            else
            {
                Console.WriteLine("Database or Books DbSet is null.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting book: {ex.Message}");
        }
    }

    private void ResetFields()
    {
        Title = string.Empty;
        Author = string.Empty;
        Publisher = string.Empty;
        Year = 0;
        Stock = 0;
        SelectedBook = null;
    }
}