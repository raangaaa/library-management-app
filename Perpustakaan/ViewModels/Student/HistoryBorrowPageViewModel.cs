using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Perpustakaan.Models;
using Perpustakaan.Services;

namespace Perpustakaan.ViewModels.Student;

public partial class HistoryBorrowPageViewModel : ViewModelBase 
{ 
    public ObservableCollection<BorrowModel> Borrows { get; } = [];

    public HistoryBorrowPageViewModel() 
    {
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
                        .Include(b => b.User!)
                            .ThenInclude(u => u.Student)
                        .Include(b => b.BorrowBooks!)
                            .ThenInclude(bb => bb.Book)
                        .Where(b => b.User_Id == AuthState.CurrentUser!.User_Id)
                        .ToListAsync();

                foreach (var borrow in borrows)
                {
                    Borrows.Add(borrow);
                }
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
}