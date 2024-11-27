using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Perpustakaan.Models;
using Perpustakaan.Services;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;

namespace Perpustakaan.ViewModels;

public partial class HomePageViewModel : ViewModelBase 
{ 
    [ObservableProperty]
    private UserModel? _user;
    [ObservableProperty]
    private int _bookCount;
    [ObservableProperty]
    private int _borrowCount;
    [ObservableProperty]
    private int _studentCount;
    [ObservableProperty]
    private ISeries[] _series;

    public HomePageViewModel() 
    {
        User = AuthState.CurrentUser;

        _ = LoadBooks();
        _ = LoadBorrows();
        _ = LoadStudents();

        Series =
        [
            new LineSeries<ObservablePoint>
            {
                Values = new ObservableCollection<ObservablePoint>
                {
                    new(0, 4),
                    new(1, 3),
                    new(3, 8),
                    new(18, 6),
                    new(20, 12)
                }
            }
        ];
    }

    private async Task LoadBooks()
    {
        try
        {
            using var db = new DatabaseService();
            if (db?.Books != null)
            {
                var books = await db.Books.CountAsync();
                BookCount = books;
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

    private async Task LoadBorrows()
    {
        try
        {
            using var db = new DatabaseService();
            if (db?.Borrows != null)
            {
                var borrows = await db.Borrows.CountAsync();
                BorrowCount = borrows;

            }
            else
            {
                Console.WriteLine("Database or Borrows DbSet is null.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading borrows: {ex.Message}");
        }
    }

    private async Task LoadStudents()
    {
        try
        {
            using var db = new DatabaseService();
            if (db?.Users != null)
            {
                var students = await db.Users.CountAsync();
                StudentCount = students;
            }
            else
            {
                Console.WriteLine("Database or Students DbSet is null.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading students: {ex.Message}");
        }
    }

}