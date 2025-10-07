using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryApp
{
    // Перечисление жанров
    public enum Genre
    {
        Fiction = 1,       // Художественная литература
        Science,           // Научная
        History,           // Историческая
        Fantasy,           // Фэнтези
        Biography          // Биография
    }
    public class Book
    {
        private static int nextId = 1;

        public int Id { get; }
        public string Title { get; set; }
        public string Author { get; set; }
        public Genre Genre { get; set; }
        public int Year { get; set; }
        public decimal Price { get; set; }

        public Book(string title, string author, Genre genre, int year, decimal price)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Название книги не может быть пустым");
            if (string.IsNullOrWhiteSpace(author))
                throw new ArgumentException("Автор не может быть пустым");
            if (year < 0)
                throw new ArgumentException("Год не может быть отрицательным");
            if (price < 0)
                throw new ArgumentException("Цена не может быть отрицательной");

            Id = nextId++;
            Title = title;
            Author = author;
            Genre = genre;
            Year = year;
            Price = price;
        }

        public override string ToString()
        {
            return $"ID: {Id}\nНазвание: {Title}\nАвтор: {Author}\nЖанр: {Genre}\nГод: {Year}\nЦена: {Price} руб.\n";
        }
    }