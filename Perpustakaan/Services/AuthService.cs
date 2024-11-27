using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Perpustakaan.Models;

namespace Perpustakaan.Services;

public class AuthService
{

    public async static Task<UserModel> AuthenticateAsync(string username, string password)
    {
        using var db = new DatabaseService();
        if (db?.Users != null)
        {
            var user = await db.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            if (user == null) return null!;
            if (user != null) return user;

        }
        else
        {
            Console.WriteLine("Database or User DbSet is null.");
        }
        return null!;
    }
}

public static class AuthState
{
    public static UserModel? CurrentUser { get; set; }
}

