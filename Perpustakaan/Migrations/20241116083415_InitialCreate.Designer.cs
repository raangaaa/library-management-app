﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Perpustakaan.Services;

#nullable disable

namespace Perpustakaan.Migrations
{
    [DbContext(typeof(DatabaseService))]
    [Migration("20241116083415_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Perpustakaan.Models.BookModel", b =>
                {
                    b.Property<int>("BookId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("BookId"));

                    b.Property<string>("Author")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Publisher")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Stock")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Year")
                        .HasColumnType("int");

                    b.HasKey("BookId");

                    b.ToTable("Books");
                });

            modelBuilder.Entity("Perpustakaan.Models.BorrowBookModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("BookId")
                        .HasColumnType("int");

                    b.Property<int>("NIS")
                        .HasColumnType("int");

                    b.Property<string>("StudentNIS")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int?>("UserModelUserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BookId");

                    b.HasIndex("StudentNIS");

                    b.HasIndex("UserModelUserId");

                    b.ToTable("BorrowBooks");
                });

            modelBuilder.Entity("Perpustakaan.Models.StudentModel", b =>
                {
                    b.Property<string>("NIS")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Class")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Phone")
                        .HasColumnType("int");

                    b.HasKey("NIS");

                    b.ToTable("Students");
                });

            modelBuilder.Entity("Perpustakaan.Models.UserModel", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserId"));

                    b.Property<string>("Password")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Username")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Perpustakaan.Models.BorrowBookModel", b =>
                {
                    b.HasOne("Perpustakaan.Models.BookModel", "Book")
                        .WithMany("BorrowBooks")
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Perpustakaan.Models.StudentModel", "Student")
                        .WithMany("BorrowBooks")
                        .HasForeignKey("StudentNIS");

                    b.HasOne("Perpustakaan.Models.UserModel", null)
                        .WithMany("BorrowBooks")
                        .HasForeignKey("UserModelUserId");

                    b.Navigation("Book");

                    b.Navigation("Student");
                });

            modelBuilder.Entity("Perpustakaan.Models.BookModel", b =>
                {
                    b.Navigation("BorrowBooks");
                });

            modelBuilder.Entity("Perpustakaan.Models.StudentModel", b =>
                {
                    b.Navigation("BorrowBooks");
                });

            modelBuilder.Entity("Perpustakaan.Models.UserModel", b =>
                {
                    b.Navigation("BorrowBooks");
                });
#pragma warning restore 612, 618
        }
    }
}
