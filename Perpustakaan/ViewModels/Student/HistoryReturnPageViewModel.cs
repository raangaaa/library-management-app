using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Perpustakaan.Models;
using Perpustakaan.Services;

namespace Perpustakaan.ViewModels.Student;

public partial class HistoryReturnPageViewModel : ViewModelBase 
{ 
    public ObservableCollection<ReturnModel> Returns { get; } = [];

    public HistoryReturnPageViewModel() 
    {
        _ = LoadReturns();
    }
    
    private async Task LoadReturns()
    {
        try
        {
            using var db = new DatabaseService();
            if (db?.Return != null)
            {
                Returns.Clear();
                var returns = await db.Return
                        .Include(r => r.Book)
                        .Where(b => b.Borrow!.User_Id == AuthState.CurrentUser!.User_Id)
                        .ToListAsync();

                foreach (var returned in returns)
                {
                    Returns.Add(returned);
                }
            }
            else
            {
                Console.WriteLine("Database or Returns DbSet is null.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading returneds: {ex.Message}");
        }
    }
}