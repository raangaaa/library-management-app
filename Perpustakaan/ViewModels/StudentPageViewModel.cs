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

public partial class StudentPageViewModel : ViewModelBase
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
    private string? _nis;
    [ObservableProperty]
    private string? _name;
    [ObservableProperty]
    private string? _class;
    [ObservableProperty]
    private string? _address;
    [ObservableProperty]
    private string? _phone;
    [ObservableProperty]
    private string? _email;
    [ObservableProperty]
    private string? _username;
    [ObservableProperty]
    private string? _password;

    [ObservableProperty]
    private UserModel? _selectedStudent;
    public ObservableCollection<UserModel> Students { get; } = [];
    public ObservableCollection<string> Errors { get; } = [];

    public IAsyncRelayCommand SaveStudentCommand { get; }
    public IAsyncRelayCommand UpdateStudentCommand { get; }
    public IAsyncRelayCommand DeleteStudentCommand { get; }
    public IAsyncRelayCommand LoadStudentsCommand { get; }
    public IAsyncRelayCommand SearchStudentsCommand { get; }

    public StudentPageViewModel()
    {
        SaveStudentCommand = new AsyncRelayCommand(SaveStudent);
        UpdateStudentCommand = new AsyncRelayCommand(UpdateStudent);
        DeleteStudentCommand = new AsyncRelayCommand(DeleteStudent);
        LoadStudentsCommand = new AsyncRelayCommand(LoadStudents);
        SearchStudentsCommand = new AsyncRelayCommand(SearchStudents);

        _ = LoadStudents();
    }

    private async Task SearchStudents()
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
            if (db?.Users != null && db?.Students != null)
            {
                Students.Clear();
                var users = await db.Users
                    .Where(b => b.Name!.ToLower().Contains(Search))
                    .Include(u => u.Student)
                    .ToListAsync();

                var students = await db.Students
                    .Where(b => EF.Functions.Like(b.NIS, $"%{Search}%"))
                    .ToListAsync();

                foreach (var user in users)
                {
                    Students.Add(user);
                }

                foreach (var student in students)
                {
                    var user = await db.Users
                        .Include(u => u.Student)
                        .FirstOrDefaultAsync(u => u.User_Id == student.User_Id);

                    if (user == null)
                    {
                        continue;
                    }
                    Students.Add(user);
                }

            }
            else
            {
                Console.WriteLine("Database or Students DbSet is null.");
            }
        }
        catch (Exception ex)
        {
            Errors.Clear();
            Errors.Add($"Error loading students: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine(ex.Message);
        }

    }

    private async Task LoadStudents()
    {
        try
        {
            using var db = new DatabaseService();
            if (db?.Users != null)
            {
                Students.Clear();
                var students = await db.Users
                        .Include(u => u.Student)
                        .ToListAsync();

                foreach (var student in students)
                {
                    Students.Add(student);
                }
            }
            else
            {
                Console.WriteLine("Database or Students DbSet is null.");
            }
        }
        catch (Exception ex)
        {
            Errors.Clear();
            Errors.Add($"Error loading students: {ex.Message}");
        }
    }

    private async Task SaveStudent()
    {
        var lifetime = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var window = lifetime?.Windows?.FirstOrDefault(x => x is MainWindow) as MainWindow;

        var student = new StudentModel
        {
            NIS = Nis,
            Class = Class,
            Address = Address
        };

        var user =  new UserModel
        {
            Name = Name,
            Role = Roles.Student,
            Email = Email,
            Phone = Phone,
            Username = Username,
            Password = Password,
            Student = student
        };

        var validatorUser = new UserModelValidator();
        var validatorStudent = new StudentModelValidator();
        var resultsUser = validatorUser.Validate(user);
        var resultsStudent = validatorUser.Validate(user);

        if (!resultsUser.IsValid)
        {
            Errors.Clear();
            foreach (var failure in resultsUser.Errors)
            {
                Errors.Add($"Column {failure.PropertyName} failed validation. Error: {failure.ErrorMessage}");
            }
            return;
        }

        if (!resultsStudent.IsValid)
        {
            Errors.Clear();
            foreach (var failure in resultsStudent.Errors)
            {
                Errors.Add($"Column {failure.PropertyName} failed validation. Error: {failure.ErrorMessage}");
            }
            return;
        }

        try
        {
            using var db = new DatabaseService();
            if (db?.Users != null)
            {
                await db.Users.AddAsync(user);
                await db.SaveChangesAsync();
                Students.Add(user);

                ResetFields();
                
                window?.NotificationService.Show("Success", "Student successfully added!");
            }
            else
            {
                Console.WriteLine("Database or Students DbSet is null.");
            }
        }
        catch (Exception ex)
        {
            Errors.Clear();
            Errors.Add($"Error: {ex.Message}");
        }
    }

    private async Task UpdateStudent()
    {
        var lifetime = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var window = lifetime?.Windows?.FirstOrDefault(x => x is MainWindow) as MainWindow;

        if (SelectedStudent == null)
        {
            window?.NotificationService.Show("Error", "No student selected for update!");
            return;
        }

        var editUser =  new UserModel
        {
            Name = Name,
            Role = Roles.Student,
            Email = Email,
            Phone = Phone,
            Username = Username,
            Password = Password,
            Student = new StudentModel
            {
                NIS = Nis,
                Class = Class,
                Address = Address
            }
        };

        var validatorUser = new UserModelValidator();
        var results = validatorUser.Validate(editUser);

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
            if (db?.Users != null)
            {
                var user = await db.Users
                    .Include(u => u.Student)
                    .FirstOrDefaultAsync(u => u.User_Id == SelectedStudent.User_Id);

                if (user != null)
                {
                    user.Name = Name;
                    user.Role = Roles.Student;
                    user.Email = Email;
                    user.Phone = Phone;
                    user.Username = Username;
                    user.Password = Password;

                    if (user.Student != null)
                    {
                        user.Student.NIS = Nis;
                        user.Student.Class = Class;
                        user.Student.Address = Address;
                    }
                    else
                    {
                        user.Student = new StudentModel
                        {
                            User_Id = user.User_Id,
                            NIS = Nis,
                            Class = Class,
                            Address = Address
                        };
                    }

                    await db.SaveChangesAsync();
                    await LoadStudents();

                    window?.NotificationService.Show("Update", "Selected student successfully updated!");
                }
                else
                {
                    window?.NotificationService.Show("Error", "Student not found!");
                }
            }
            else
            {
                Console.WriteLine("Database or Students DbSet is null.");
            }
        }
        catch (Exception ex)
        {
            Errors.Clear();
            Errors.Add($"Error updating student: {ex.Message}");
        }
    }

    private async Task DeleteStudent()
    {
        var lifetime = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var window = lifetime?.Windows?.FirstOrDefault(x => x is MainWindow) as MainWindow;

        if (SelectedStudent == null)
        {
            window?.NotificationService.Show("Error", "No student selected for delete!");
            return;
        }

        try
        {
            using var db = new DatabaseService();
            if (db.Users != null)
            {
                var user = await db.Users
                    .Include(u => u.Student)
                    .FirstOrDefaultAsync(u => u.User_Id == SelectedStudent.User_Id);

                if (user != null)
                {
                    db.Users.Remove(user);
                    await db.SaveChangesAsync();
                    Students.Remove(SelectedStudent);
                    ResetFields();

                    window?.NotificationService.Show("Delete", "Selected student was deleted!");
                } 
                else
                {
                    window?.NotificationService.Show("Error", "Student not found!");
                }
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
        }
    }



    partial void OnSelectedStudentChanged(UserModel? value)
    {
        EditFields();
    }

    private void EditFields()
    {
        Name = SelectedStudent?.Name;
        Email = SelectedStudent?.Email;
        Phone = SelectedStudent?.Phone;
        Username = SelectedStudent?.Username;
        Password = SelectedStudent?.Password;
        Nis = SelectedStudent?.Student?.NIS;
        Class = SelectedStudent?.Student?.Class;
        Address = SelectedStudent?.Student?.Address;
    }

    [RelayCommand]
    public void ResetFields()
    {
        Name = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        Username = string.Empty;
        Password = string.Empty;
        Nis = string.Empty;
        Class = string.Empty;
        Address = string.Empty;
        SelectedStudent = null;
    }
}