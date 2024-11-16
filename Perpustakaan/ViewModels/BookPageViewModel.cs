using System.Collections.Generic;
using System.Collections.ObjectModel;
using Perpustakaan.Models;

namespace Perpustakaan.ViewModels;

public class BookPageViewModel : ViewModelBase 
{ 
    public ObservableCollection<Book> Books { get; }

    public BookPageViewModel()
    {
        var people = new List<Book> 
        {
            new(1, "makan", "makan","makan", 1020, 10),
             new(2, "makan", "makan","makan", 1020, 10),
              new(3, "makan", "makan","makan", 1020, 10),
               new(4, "makan", "makan","makan", 1020, 10),
        };
        Books = [.. people];
    }
}

public class Book(int id, string title, string author, string publisher, int year, int stock)
{
    public int BookId { get; set; } = id;
    public string? Title { get; set; } = title;
    public string? Author { get; set; } = author;
    public string? Publisher { get; set; } = publisher;
    public int Year { get; set; } = year;
    public int Stock { get; set; } = stock;
}