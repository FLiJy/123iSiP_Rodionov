// Core (класс доступа к контексту)
// 1. Инициализировать единый статический экземпляр контекста EF для всего приложения.
// 2. Обеспечить доступ к таблицам через Core.Context из других методов.
// 3. (Не хранить долгоживущие транзакции — закрывать/пересоздавать контекст при необходимости в больших проектах).

// Main
// 1. Настроить консоль (заголовок, кодировка).
// 2. Показать главное меню: вход, регистрация, просмотр товаров как гость, выход.
// 3. Считать выбор пользователя.
// 4. Вызвать соответствующий метод (Login, Register, ShowProducts) в зависимости от выбора.
// 5. Завершить программу или вернуться в меню по результату действий.

// Register
// 1. Открыть экран регистрации (очистить консоль и вывести заголовок).
// 2. Запросить никнейм у пользователя.
// 3. Проверить в базе, занят ли ник (по Core.Context.Users).
// 4. Если занят — сообщить и предложить другой или вернуться.
// 5. Попросить ввести пароль и подтвердить (ввести дважды).
// 6. Сверить два введённых пароля.
// 7. Если пароли совпадают — создать объект Users и добавить в контекст.
// 8. Вызвать SaveChanges() для сохранения в БД.
// 9. Выдать сообщение об успехе и перейти в UserMenu под новым пользователем.

// Login
// 1. Открыть экран входа (очистить консоль и вывести заголовок).
// 2. Запросить никнейм и пароль.
// 3. Найти пользователя в базе по никнейму и паролю (Core.Context.Users).
// 4. Если пользователь не найден — вывести ошибку и вернуть в главное меню.
// 5. Если найден — показать приветствие и вызвать UserMenu для этого пользователя.

// UserMenu
// 1. Вывести персональное меню пользователя (товары, корзина, история, выход).
// 2. Считать выбор пользователя в цикле, пока он не выйдет.
// 3. По выбору вызвать соответствующий метод: ShowProducts, ShowCart, ShowOrderHistory.
// 4. При выборе выйти — завершить цикл и вернуться в Main.

// ShowProducts
// 1. Очистить консоль и вывести заголовок "Список товаров".
// 2. Получить все товары из Core.Context.Products.
// 3. Вывести список товаров с Id, именем и ценой.
// 4. Если метод вызывается под залогиненным пользователем:
//    4.1. Предложить ввести ID для добавления товара в корзину или 0 для выхода.
//    4.2. Проверить корректность ввода (int, существующий товар, достаточное количество).
//    4.3. Добавить или обновить запись в таблице Cart (Core.Context.Cart или CartItems).
//    4.4. Сохранить изменения в БД (SaveChanges).
// 5. Если гость — только просмотр, без возможности покупки.
// 6. Вернуться в предыдущее меню.

// ShowCart
// 1. Очистить консоль и показать заголовок корзины.
// 2. Загрузить все элементы корзины пользователя из Core.Context.Cart по user.Id.
// 3. Если корзина пуста — вывести сообщение и вернуть управление.
// 4. Вывести каждый элемент: имя товара, цена, количество, суммарная цена.
// 5. Показать действия: купить один товар, купить всё, очистить корзину, назад.
// 6. Считать выбор и вызвать BuySingleItem / BuyAllItems / ClearCart по необходимости.

// BuySingleItem
// 1. Попросить ввести номер (индекс) товара из отображённой корзины.
// 2. Проверить корректность индекса и получить соответствующий элемент Cart.
// 3. Получить данные самого продукта (Core.Context.Products) для проверки остатков и цены.
// 4. Запросить у пользователя информацию о ПВЗ (можно выбрать из таблицы PickupPoints или ввести строку).
// 5. Создать новую запись заказа (Orders) с привязкой к пользователю, товару, количеству, дате и ПВЗ.
// 6. Удалить купленный элемент из корзины или уменьшить его количество при частичной покупке.
// 7. Уменьшить количество товара на складе (Products.Quantity) при необходимости.
// 8. Сохранить изменения вызовом SaveChanges().
// 9. Вывести подтверждение покупки и вернуться в ShowCart.

// BuyAllItems (CheckoutCart / BuyAllItems)
// 1. Получить все элементы корзины текущего пользователя.
// 2. Если корзина пуста — сообщить и выйти.
// 3. Попросить пользователя выбрать ПВЗ (или ввести текстовое поле ПВЗ).
// 4. Для каждого элемента корзины:
//    4.1. Проверить доступность товара (Products.Quantity >= требуемого количества).
//    4.2. Создать запись Orders (или одну общую заявку с несколькими OrderItems в более правильной схеме).
//    4.3. Уменьшить количество на складе у Products соответственно.
// 5. Удалить все элементы корзины пользователя (RemoveRange / Delete).
// 6. Выполнить SaveChanges() один раз после всех изменений.
// 7. Вывести итоговое сообщение об успешной покупке.

// ClearCart
// 1. Найти все записи корзины текущего пользователя в Core.Context.Cart.
// 2. Удалить их (RemoveRange).
// 3. Вызвать SaveChanges().
// 4. Сообщить пользователю, что корзина очищена и вернуться в меню.

// ShowOrderHistory
// 1. Очистить консоль и вывести заголовок "История заказов".
// 2. Загрузить все заказы пользователя из Core.Context.Orders, при необходимости с join`ами к Products и PickupPoints.
// 3. Отсортировать по дате (по умолчанию — от новых к старым).
// 4. Если заказов нет — вывести соответствующее сообщение.
// 5. Для каждого заказа вывести: дата, список товаров (или название одного товара), количество, итоговую цену, ПВЗ.
// 6. Предложить опцию сортировки: "От старых к новым" / "От новых к старым" — и повторно отобразить список.
// 7. Вернуть управление в UserMenu.

// Вспомогательные замечания (рекомендуемые проверки в методах)
// 1. Всегда валидировать пользовательский ввод (int.TryParse, проверки границ и существования записей).
// 2. Обрабатывать возможные исключения при работе с БД (try/catch при SaveChanges).
// 3. Не держать состояние в статическом контексте слишком долго для реальных приложений — здесь допустимо.
// 4. Для истории и отчетов удобнее иметь таблицы Order и OrderItems — тогда в ShowOrderHistory делать join на OrderItems.
// 5. Логирование (Console.WriteLine) ошибок и операций поможет при отладке.
using _8PRAC;
using PRAC8;
using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        Console.Title = "GMWOG Marketplace";
        Console.WriteLine("Добро пожаловать в онлайн-маркетплейс GMWOG!");
        Console.WriteLine("1 - Войти");
        Console.WriteLine("2 - Зарегистрироваться");
        Console.WriteLine("3 - Смотреть товары (гость)");
        Console.Write("Выбор: ");
        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                Login();
                break;
            case "2":
                Register();
                break;
            case "3":
                ShowProducts(null);
                break;
            default:
                Console.WriteLine("Неверный выбор!");
                break;
        }

        Console.WriteLine("\nНажмите любую клавишу для выхода...");
        Console.ReadKey();
    }

    // Регистрация нового пользователя
    static void Register()
    {
        Console.Clear();
        Console.WriteLine("=== Регистрация ===");

        Console.Write("Введите никнейм: ");
        string username = Console.ReadLine();

        // Проверка, не занят ли ник
        if (Core.Context.Users.Any(u => u.Username == username))
        {
            Console.WriteLine("Пользователь с таким ником уже существует!");
            return;
        }

        Console.Write("Введите пароль: ");
        string pass1 = Console.ReadLine();
        Console.Write("Подтвердите пароль: ");
        string pass2 = Console.ReadLine();

        if (pass1 != pass2)
        {
            Console.WriteLine("Пароли не совпадают!");
            return;
        }

        var newUser = new Users
        {
            Username = username,
            Password = pass1
        };

        Core.Context.Users.Add(newUser);
        Core.Context.SaveChanges();
        Console.WriteLine("Регистрация успешна!");

        UserMenu(newUser);
    }

    // Авторизация
    static void Login()
    {
        Console.Clear();
        Console.WriteLine("=== Вход ===");
        Console.Write("Введите никнейм: ");
        string username = Console.ReadLine();
        Console.Write("Введите пароль: ");
        string password = Console.ReadLine();

        var user = Core.Context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
        if (user == null)
        {
            Console.WriteLine("Неверный логин или пароль!");
            return;
        }

        Console.WriteLine($"Добро пожаловать, {user.Username}!");
        UserMenu(user);
    }

    // Меню пользователя
    static void UserMenu(Users user)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"=== Меню пользователя: {user.Username} ===");
            Console.WriteLine("1 - Смотреть товары");
            Console.WriteLine("2 - Посмотреть корзину");
            Console.WriteLine("3 - История заказов");
            Console.WriteLine("4 - Выйти");
            Console.Write("Выбор: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ShowProducts(user);
                    break;
                case "2":
                    ShowCart(user);
                    break;
                case "3":
                    ShowOrderHistory(user);
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Неверный выбор!");
                    break;
            }
        }
    }

    // Просмотр товаров
    static void ShowProducts(Users user)
    {
        Console.Clear();
        Console.WriteLine("=== Список товаров ===");

        var products = Core.Context.Products.ToList();
        if (products.Count == 0)
        {
            Console.WriteLine("Нет доступных товаров.");
            return;
        }

        foreach (var p in products)
        {
            Console.WriteLine($"{p.Id}. {p.Name} - {p.Price} руб.");
        }

        if (user != null)
        {
            Console.WriteLine("\nВведите ID товара, чтобы добавить в корзину (или 0 для выхода): ");
            if (int.TryParse(Console.ReadLine(), out int id) && id > 0)
            {
                var product = Core.Context.Products.FirstOrDefault(p => p.Id == id);
                if (product != null)
                {
                    Core.Context.Cart.Add(new Cart
                    {
                        UserId = user.Id,
                        ProductId = product.Id,
                        Quantity = 1
                    });
                    Core.Context.SaveChanges();
                    Console.WriteLine($"{product.Name} добавлен в корзину!");
                }
            }
        }

        Console.WriteLine("\nНажмите любую клавишу для возврата...");
        Console.ReadKey();
    }

    // Просмотр корзины
    static void ShowCart(Users user)
    {
        Console.Clear();
        Console.WriteLine("=== Ваша корзина ===");

        var cartItems = Core.Context.Cart.Where(c => c.UserId == user.Id).ToList();
        if (cartItems.Count == 0)
        {
            Console.WriteLine("Корзина пуста.");
            Console.ReadKey();
            return;
        }

        int i = 1;
        foreach (var item in cartItems)
        {
            var prod = Core.Context.Products.First(p => p.Id == item.ProductId);
            Console.WriteLine($"{i}. {prod.Name} - {prod.Price} руб. (x{item.Quantity})");
            i++;
        }

        Console.WriteLine("\n1 - Купить один товар");
        Console.WriteLine("2 - Купить всё");
        Console.WriteLine("3 - Очистить корзину");
        Console.WriteLine("4 - Назад");
        Console.Write("Выбор: ");
        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                BuySingleItem(user, cartItems);
                break;
            case "2":
                BuyAllItems(user, cartItems);
                break;
            case "3":
                ClearCart(user);
                break;
        }
    }

    // Покупка одного товара
    static void BuySingleItem(Users user, List<Cart> cartItems)
    {
        Console.Write("Введите номер товара из корзины: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= cartItems.Count)
        {
            var item = cartItems[index - 1];
            var prod = Core.Context.Products.First(p => p.Id == item.ProductId);

            Console.Write("Введите ПВЗ (например: Москва, ТЦ Европа): ");
            string pvz = Console.ReadLine();

            var order = new Orders
            {
                UserId = user.Id,
                ProductId = prod.Id,
                Quantity = item.Quantity,
                OrderDate = DateTime.Now,
                PickupPoint = pvz
            };

            Core.Context.Orders.Add(order);
            Core.Context.Cart.Remove(item);
            Core.Context.SaveChanges();

            Console.WriteLine($"Вы купили {prod.Name}!");
        }

        Console.ReadKey();
    }

    // Покупка всех товаров
    static void BuyAllItems(Users user, List<Cart> cartItems)
    {
        Console.Write("Введите ПВЗ для всех товаров: ");
        string pvz = Console.ReadLine();

        foreach (var item in cartItems)
        {
            var prod = Core.Context.Products.First(p => p.Id == item.ProductId);

            var order = new Orders
            {
                UserId = user.Id,
                ProductId = prod.Id,
                Quantity = item.Quantity,
                OrderDate = DateTime.Now,
                PickupPoint = pvz
            };

            Core.Context.Orders.Add(order);
        }

        Core.Context.Cart.RemoveRange(cartItems);
        Core.Context.SaveChanges();

        Console.WriteLine("Все товары куплены!");
        Console.ReadKey();
    }

    // Очистка корзины
    static void ClearCart(Users user)
    {
        var items = Core.Context.Cart.Where(c => c.UserId == user.Id).ToList();
        Core.Context.Cart.RemoveRange(items);
        Core.Context.SaveChanges();
        Console.WriteLine("Корзина очищена!");
        Console.ReadKey();
    }

    // История заказов
    static void ShowOrderHistory(Users user)
    {
        Console.Clear();
        Console.WriteLine("=== История заказов ===");

        var orders = Core.Context.Orders
            .Where(o => o.UserId == user.Id)
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        if (orders.Count == 0)
        {
            Console.WriteLine("У вас пока нет заказов.");
        }
        else
        {
            foreach (var o in orders)
            {
                var prod = Core.Context.Products.First(p => p.Id == o.ProductId);
                Console.WriteLine($"{prod.Name} x{o.Quantity} — {o.OrderDate} — ПВЗ: {o.PickupPoint}");
            }
        }

        Console.WriteLine("\n1 - От старых к новым");
        Console.WriteLine("2 - От новых к старым");
        Console.WriteLine("3 - Назад");
        Console.Write("Выбор: ");
        string choice = Console.ReadLine();

        if (choice == "1")
        {
            var sorted = orders.OrderBy(o => o.OrderDate).ToList();
            Console.Clear();
            Console.WriteLine("=== От старых к новым ===");
            foreach (var o in sorted)
            {
                var prod = Core.Context.Products.First(p => p.Id == o.ProductId);
                Console.WriteLine($"{prod.Name} — {o.OrderDate}");
            }
        }

        Console.WriteLine("\nНажмите любую клавишу для возврата...");
        Console.ReadKey();
    }
}
