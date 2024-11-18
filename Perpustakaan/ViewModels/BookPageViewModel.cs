using System;
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
            new(1, "Abdu", "makan","makan", 1020, 10),
            new(2, "Budi", "makan","makan", 1020, 10),
            new(3, "Juni", "makan","makan", 1020, 10),
            new(4, "Kayla", "makan","makan", 1020, 10),
            new(5, "Soni", "makan","makan", 1020, 10),
            new(6, "Makala", "makan","makan", 1020, 10),
            new(7, "Joni", "makan","makan", 1020, 10),
            new(8, "Kyu", "makan","makan", 1020, 10),
            new(9, "Ryu", "makan","makan", 1020, 10),
            new(10, "Soi", "makan","makan", 1020, 10),
        };
        Books = [.. people];

        // Debug jumlah item
        Console.WriteLine($"Jumlah item di Books: {Books.Count}");
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