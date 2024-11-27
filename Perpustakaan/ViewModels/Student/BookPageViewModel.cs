using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Perpustakaan.Models;
using Perpustakaan.Services;

namespace Perpustakaan.ViewModels.Student;

public partial class BookPageViewModel : ViewModelBase 
{ 
    public ObservableCollection<BookModel> Books { get; } = [];

    public BookPageViewModel() 
    {
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
}