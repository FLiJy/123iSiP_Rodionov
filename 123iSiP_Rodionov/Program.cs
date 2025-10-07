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
    class Program
    {
        static List<Book> books = new List<Book>();

        static void Main()
        {
            // Добавляем тестовые данные
            books.Add(new Book("Война и мир", "Лев Толстой", Genre.History, 1869, 1200));
            books.Add(new Book("1984", "Джордж Оруэлл", Genre.Fiction, 1949, 800));
            books.Add(new Book("Гарри Поттер", "Дж. Роулинг", Genre.Fantasy, 1997, 1000));
            books.Add(new Book("Краткая история времени", "Стивен Хокинг", Genre.Science, 1988, 1500));
            books.Add(new Book("Стив Джобс", "Уолтер Айзексон", Genre.Biography, 2011, 1100));

            while (true)
            {
                Console.WriteLine("\n--- БИБЛИОТЕКА ---");
                Console.WriteLine("1. Добавить книгу");
                Console.WriteLine("2. Удалить книгу по ID");
                Console.WriteLine("3. Найти книги");
                Console.WriteLine("4. Отсортировать книги");
                Console.WriteLine("5. Самая дорогая и самая дешёвая книга");
                Console.WriteLine("6. Сгруппировать книги по авторам");
                Console.WriteLine("7. Показать все книги");
                Console.WriteLine("0. Выйти");
                Console.Write("Выберите команду: ");

                var input = Console.ReadLine();
                Console.Clear();

                switch (input)
                {
                    case "1": AddBook(); break;
                    case "2": DeleteBook(); break;
                    case "3": SearchBooks(); break;
                    case "4": SortBooks(); break;
                    case "5": ShowPriceExtremes(); break;
                    case "6": GroupByAuthors(); break;
                    case "7": ShowAllBooks(); break;
                    case "0": return;
                    default: Console.WriteLine("Неверный выбор."); break;
                }
            }
        }
        static void AddBook()
        {
            try
            {
                Console.Write("Введите название: ");
                string title = Console.ReadLine();

                Console.Write("Введите автора: ");
                string author = Console.ReadLine();

                Console.WriteLine("Выберите жанр:");
                foreach (var g in Enum.GetValues(typeof(Genre)))
                    Console.WriteLine($"{(int)g}. {g}");

                if (!int.TryParse(Console.ReadLine(), out int genreChoice) || !Enum.IsDefined(typeof(Genre), genreChoice))
                {
                    Console.WriteLine("Неверный выбор жанра.");
                    return;
                }

                Genre genre = (Genre)genreChoice;

                Console.Write("Введите год издания: ");
                if (!int.TryParse(Console.ReadLine(), out int year) || year <= 0)
                {
                    Console.WriteLine("Некорректный год.");
                    return;
                }

                Console.Write("Введите цену: ");
                if (!decimal.TryParse(Console.ReadLine(), out decimal price) || price < 0)
                {
                    Console.WriteLine("Некорректная цена.");
                    return;
                }

                books.Add(new Book(title, author, genre, year, price));
                Console.WriteLine("Книга успешно добавлена!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }