using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryApp
{
    public enum Genre
    {
        Fiction = 1,       
        Science,           
        History,           
        Fantasy,           
        Biography          
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
        static void DeleteBook()
        {
            Console.Write("Введите ID книги для удаления: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var book = books.FirstOrDefault(b => b.Id == id);
                if (book != null)
                {
                    books.Remove(book);
                    Console.WriteLine("Книга удалена.");
                }
                else Console.WriteLine("Книга с таким ID не найдена.");
            }
            else Console.WriteLine("Неверный ID.");
        }
        static void SearchBooks()
        {
            Console.WriteLine("1. По названию");
            Console.WriteLine("2. По автору");
            Console.WriteLine("3. По жанру");
            Console.Write("Выберите способ поиска: ");
            string choice = Console.ReadLine();

            IEnumerable<Book> result = Enumerable.Empty<Book>();

            switch (choice)
            {
                case "1":
                    Console.Write("Введите название: ");
                    string title = Console.ReadLine();
                    result = books.Where(b => b.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
                    break;
                case "2":
                    Console.Write("Введите автора: ");
                    string author = Console.ReadLine();
                    result = books.Where(b => b.Author.Contains(author, StringComparison.OrdinalIgnoreCase));
                    break;
                case "3":
                    Console.WriteLine("Выберите жанр:");
                    foreach (var g in Enum.GetValues(typeof(Genre)))
                        Console.WriteLine($"{(int)g}. {g}");
                    if (int.TryParse(Console.ReadLine(), out int gChoice) && Enum.IsDefined(typeof(Genre), gChoice))
                    {
                        Genre g = (Genre)gChoice;
                        result = books.Where(b => b.Genre == g);
                    }
                    else Console.WriteLine("Неверный выбор жанра.");
                    break;
                default:
                    Console.WriteLine("Неверный выбор.");
                    return;
            }

            if (result.Any())
                foreach (var b in result) Console.WriteLine(b);
            else
                Console.WriteLine("Книги не найдены.");
        }
        static void SortBooks()
        {
            Console.WriteLine("1. По названию");
            Console.WriteLine("2. По году");
            Console.Write("Выберите вариант сортировки: ");
            string choice = Console.ReadLine();

            IEnumerable<Book> sorted = choice switch
            {
                "1" => books.OrderBy(b => b.Title),
                "2" => books.OrderBy(b => b.Year),
                _ => Enumerable.Empty<Book>()
            };

            if (sorted.Any())
                foreach (var b in sorted) Console.WriteLine(b);
            else
                Console.WriteLine("Неверный выбор.");
        }

        static void ShowPriceExtremes()
        {
            if (books.Count == 0)
            {
                Console.WriteLine("Нет книг в списке.");
                return;
            }

            var max = books.OrderByDescending(b => b.Price).First();
            var min = books.OrderBy(b => b.Price).First();

            Console.WriteLine("Самая дорогая книга:\n" + max);
            Console.WriteLine("Самая дешёвая книга:\n" + min);
        }
        static void GroupByAuthors()
        {
            var grouped = books.GroupBy(b => b.Author)
                               .Select(g => new { Author = g.Key, Count = g.Count() });

            foreach (var g in grouped)
                Console.WriteLine($"{g.Author}: {g.Count} книг(и)");
        }
        static void ShowAllBooks()
        {
            if (books.Count == 0)
                Console.WriteLine("Нет книг в библиотеке.");
            else
                foreach (var b in books) Console.WriteLine(b);
        }
    }
}