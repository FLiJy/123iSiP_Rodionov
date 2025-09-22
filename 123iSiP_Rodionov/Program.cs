//ПР1
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        Console.WriteLine("Программа для подсчета расходов за день");

        int n;
        while (true)
        {
            Console.Write("Введите количество операций (от 2 до 40): ");
            if (int.TryParse(Console.ReadLine(), out n) && n >= 2 && n <= 40)
                break;
            else
                Console.WriteLine("Ошибка: нужно число от 2 до 40!");
        }
 
        List<(string name, double cost)> expenses = new List<(string, double)>();

        for (int i = 0; i < n; i++)
        {
            Console.WriteLine($"\nОперация №{i + 1}:");
            Console.Write("Введите название товара или услуги: ");
            string name = Console.ReadLine();

            double cost;
            while (true)
            {
                Console.Write("Введите сумму в рублях: ");
                if (double.TryParse(Console.ReadLine(), out cost))
                    break;
                else
                    Console.WriteLine("Ошибка: нужно число!");
            }

            expenses.Add((name, cost));
        }

        while (true)
        {
            Console.WriteLine("\nМеню:");
            Console.WriteLine("1. Вывод данных");
            Console.WriteLine("2. Статистика");
            Console.WriteLine("3. Сортировка по цене (пузырьком)");
            Console.WriteLine("4. Конвертация валюты");
            Console.WriteLine("5. Поиск по названию");
            Console.WriteLine("0. Выход");
            Console.Write("Выберите пункт: ");
            string choice = Console.ReadLine();

            if (choice == "0")
            {
                Console.WriteLine("Выход из программы...");
                break;
            }
            else if (choice == "1")
            {
                Console.WriteLine("\nСписок расходов:");
                foreach (var exp in expenses)
                {
                    Console.WriteLine($"- {exp.name}: {exp.cost} руб.");
                }
            }
            else if (choice == "2")
            {
                double sum = 0;
                double max = double.MinValue;
                double min = double.MaxValue;

                foreach (var exp in expenses)
                {
                    sum += exp.cost;
                    if (exp.cost > max) max = exp.cost;
                    if (exp.cost < min) min = exp.cost;
                }

                double avg = sum / expenses.Count;

                Console.WriteLine("\nСтатистика:");
                Console.WriteLine($"Сумма: {sum} руб.");
                Console.WriteLine($"Среднее: {avg} руб.");
                Console.WriteLine($"Максимум: {max} руб.");
                Console.WriteLine($"Минимум: {min} руб.");
            }
            else if (choice == "3")
            {
                for (int i = 0; i < expenses.Count - 1; i++)
                {
                    for (int j = 0; j < expenses.Count - i - 1; j++)
                    {
                        if (expenses[j].cost > expenses[j + 1].cost)
                        {
                            var temp = expenses[j];
                            expenses[j] = expenses[j + 1];
                            expenses[j + 1] = temp;
                        }
                    }
                }

                Console.WriteLine("\nСписок расходов (отсортировано по цене):");
                foreach (var exp in expenses)
                {
                    Console.WriteLine($"- {exp.name}: {exp.cost} руб.");
                }
            }
            else if (choice == "4")
            {
                Console.Write("Введите курс для конвертации (например, в доллары): ");
                double rate;
                if (double.TryParse(Console.ReadLine(), out rate))
                {
                    Console.WriteLine("\nРасходы в новой валюте:");
                    foreach (var exp in expenses)
                    {
                        Console.WriteLine($"- {exp.name}: {exp.cost / rate}");
                    }
                }
                else
                {
                    Console.WriteLine("Ошибка: нужно число!");
                }
            }
            else if (choice == "5")
            {
                Console.Write("Введите слово для поиска: ");
                string query = Console.ReadLine().ToLower();

                Console.WriteLine("\nРезультаты поиска:");
                foreach (var exp in expenses)
                {
                    if (exp.name.ToLower().Contains(query))
                    {
                        Console.WriteLine($"- {exp.name}: {exp.cost} руб.");
                    }
                }
            }
            else
            {
                Console.WriteLine("Ошибка: нет такого пункта меню!");
            }
        }
    }
}
