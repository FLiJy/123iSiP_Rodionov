//пр2  
using System;
using System.Collections.Generic;

namespace StoreInventory
{  
    public enum Category
    {
        Electronics,
        Food,
        Clothing,
        Books,
        Toys
    }
    public class Product
    {
        public int Id { get; private set; }
        public string Code => Id.ToString(); 
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public bool InStock => Quantity > 0; 
        public Category Category { get; set; }

        public Product(int id, string name, decimal price, int quantity, Category category)
        {
            Id = id;
            Name = name;
            Price = price;
            Quantity = quantity;
            Category = category;
        }
        public override string ToString()
        {
            return $"Код: {Code}\nНазвание: {Name}\nЦена: {Price:C}\nКоличество: {Quantity}\nВ наличии: {(InStock ? "Да" : "Нет")}\nКатегория: {Category}";
        }
    }
    class Program
    {
        private static List<Product> products = new List<Product>();
        private static int nextId = 1;  

        static void Main(string[] args)
        {
            InitializeTestData(); 

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Учёт товаров в магазине");
                Console.WriteLine("1. Добавить товар");
                Console.WriteLine("2. Удалить товар");
                Console.WriteLine("3. Заказать поставку");
                Console.WriteLine("4. Продать товар");
                Console.WriteLine("5. Поиск товаров");
                Console.WriteLine("0. Выход");

                string input = Console.ReadLine().Trim();
                try
                {
                    int choice = int.Parse(input);
                    switch (choice)
                    {
                        case 1: AddProduct(); break;
                        case 2: RemoveProduct(); break;
                        case 3: OrderSupply(); break;
                        case 4: SellProduct(); break;
                        case 5: SearchProducts(); break;
                        case 0: return;
                        default: Console.WriteLine("Неверный выбор."); break;
                    }
                }
                catch
                {
                    Console.WriteLine("Неверный ввод.");
                }

                Console.WriteLine("Нажмите любую клавишу для продолжения...");
                Console.ReadKey();
            }
        }

        private static void InitializeTestData()
        {
            products.Add(new Product(nextId++, "Смартфон", 29999.99m, 10, Category.Electronics));
            products.Add(new Product(nextId++, "Яблоки", 99.50m, 50, Category.Food));
            products.Add(new Product(nextId++, "Футболка", 1500.00m, 20, Category.Clothing));
            products.Add(new Product(nextId++, "Книга по C#", 2000.00m, 15, Category.Books));
            products.Add(new Product(nextId++, "Конструктор LEGO", 5000.00m, 5, Category.Toys));
        }
        private static void AddProduct()
        {
            Console.WriteLine("Добавление товара:");

            string name;
            do
            {
                Console.Write("Название: ");
                name = Console.ReadLine().Trim();
                if (string.IsNullOrEmpty(name))
                {
                    Console.WriteLine("Название не может быть пустым.");
                }
            } while (string.IsNullOrEmpty(name));
 
            decimal price;
            do
            {
                Console.Write("Цена: ");
                if (!decimal.TryParse(Console.ReadLine(), out price) || price <= 0)
                {
                    Console.WriteLine("Цена должна быть положительным числом.");
                    price = 0;
                }
            } while (price <= 0);

            int quantity;
            do
            {
                Console.Write("Количество: ");
                if (!int.TryParse(Console.ReadLine(), out quantity) || quantity < 0)
                {
                    Console.WriteLine("Количество должно быть неотрицательным целым числом.");
                    quantity = -1;
                }
            } while (quantity < 0);
 
            Category category;
            do
            {
                Console.WriteLine("Категории: " + string.Join(", ", Enum.GetNames(typeof(Category))));
                Console.Write("Категория: ");
                if (!Enum.TryParse(Console.ReadLine(), true, out category) || !Enum.IsDefined(typeof(Category), category))
                {
                    Console.WriteLine("Неверная категория.");
                }
            } while (!Enum.IsDefined(typeof(Category), category));

            var product = new Product(nextId++, name, price, quantity, category);
            products.Add(product);
            Console.WriteLine("Товар добавлен.");
        }
        private static void RemoveProduct()
        {
            Console.Write("Введите код товара для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out int id) || id < 1)
            {
                Console.WriteLine("Неверный код.");
                return;
            }

            var product = products.Find(p => p.Id == id);
            if (product == null)
            {
                Console.WriteLine("Товар не найден.");
                return;
            }

            products.Remove(product);
            Console.WriteLine("Товар удален.");
        }
  