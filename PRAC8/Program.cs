using PRAC8;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("Добро пожаловать в маркетплейс GG.MOW!");

        Users currentUser = null;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== МЕНЮ ===");
            Console.WriteLine("1 - Войти");
            Console.WriteLine("2 - Зарегистрироваться");
            Console.WriteLine("3 - Просмотреть товары");
            Console.WriteLine("4 - Выход");

            Console.Write("\nВыбор: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    currentUser = Login();
                    if (currentUser != null) UserMenu(currentUser);
                    break;
                case "2":
                    Register();
                    break;
                case "3":
                    ShowProducts();
                    Console.WriteLine("\nНажмите любую клавишу...");
                    Console.ReadKey();
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Некорректный выбор!");
                    break;
            }
        }
    }

    static void SaveChangesWithErrorHandling()
    {
        try
        {
            Core.Context.SaveChanges();
        }
        catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
        {
            Console.WriteLine($"Ошибка сохранения: {ex.InnerException?.Message}");
            throw;
        }
    }

    // === Регистрация ===
    static void Register()
    {
        Console.Clear();
        Console.WriteLine("=== Регистрация ===");
        Console.Write("Введите ник: ");
        string username = Console.ReadLine();

        if (Core.Context.Users.Any(u => u.Username == username))
        {
            Console.WriteLine("Такой пользователь уже существует.");
            Console.ReadKey();
            return;
        }

        Console.Write("Введите пароль: ");
        string pass1 = Console.ReadLine();
        Console.Write("Повторите пароль: ");
        string pass2 = Console.ReadLine();

        if (pass1 != pass2)
        {
            Console.WriteLine("Пароли не совпадают!");
            Console.ReadKey();
            return;
        }

        Core.Context.Users.Add(new Users { Username = username, Password = pass1 });
        SaveChangesWithErrorHandling();

        Console.WriteLine("Регистрация успешна!");
        Console.ReadKey();
    }

    // === Добавление в корзину ===
    static void AddToCart(Users user)
    {
        ShowProducts();
        Console.Write("\nВведите ID товара: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Некорректный ввод.");
            Console.ReadKey();
            return;
        }

        var product = Core.Context.Products.FirstOrDefault(p => p.Id == id);
        if (product == null || product.Quantity == 0)
        {
            Console.WriteLine("Товар недоступен.");
            Console.ReadKey();
            return;
        }

        Console.Write("Введите количество: ");
        if (!int.TryParse(Console.ReadLine(), out int qty) || qty <= 0 || qty > product.Quantity)
        {
            Console.WriteLine("Некорректное количество.");
            Console.ReadKey();
            return;
        }

        var existing = Core.Context.CartItems.FirstOrDefault(c => c.UserId == user.Id && c.ProductId == id);
        if (existing != null)
            existing.Quantity += qty;
        else
            Core.Context.CartItems.Add(new CartItems { UserId = user.Id, ProductId = id, Quantity = qty });

        SaveChangesWithErrorHandling();
        Console.WriteLine("Добавлено в корзину!");
        Console.ReadKey();
    }

    // Остальные методы с заменой Core.Context.SaveChanges() на SaveChangesWithErrorHandling()
    // ... (аналогично заменить во всех методах)
}