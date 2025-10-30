using _7PRAC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

class CarRepairSimulator
{
    private int balance;
    private Dictionary<string, int> stock;
    private List<SupplyOrder> suppliesInTransit;
    private Random rng;
    private int sessionId;

    public CarRepairSimulator(int initialBalance)
    {
        balance = initialBalance;
        stock = new Dictionary<string, int>();
        suppliesInTransit = new List<SupplyOrder>();
        rng = new Random();
        StartNewSession();
    }

    private void StartNewSession()
    {
        using (var context = new Rodionov7PRACTIKAEntities4())
        {
            // Создаем новую игровую сессию
            var newSession = new GameSessions
            {
                StartMoney = balance,
                CurrentMoney = balance,
                CreatedDate = DateTime.Now,
                LastUpdate = DateTime.Now
            };

            context.GameSessions.Add(newSession);
            context.SaveChanges();
            sessionId = newSession.Id;

            // Загружаем начальные детали из таблицы Parts
            var initialParts = context.Parts
                .Where(p => p.IsActive && p.InitialQuantity > 0)
                .ToList();

            foreach (var part in initialParts)
            {
                stock[part.Name] = part.InitialQuantity;
                UpdateStockInDb(part.Name, part.InitialQuantity);
            }
        }
    }

    public void StartSimulation()
    {
        Console.WriteLine("=== АВТОСЕРВИС ===");
        Console.WriteLine($"Начальный баланс: {balance} руб.");
        Console.WriteLine("Нажмите любую клавишу для начала обслуживания клиентов...");
        Console.ReadKey();

        int customerCount = 1;
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"=== КЛИЕНТ №{customerCount} ===");
            HandleIncomingSupplies();
            DisplayCurrentStatus();

            string faultyPart = PickRandomFault();
            int partCost = FetchPartCost(faultyPart);
            int fixPrice = partCost + rng.Next(200, 800);

            Console.WriteLine($"\nПоломка: {faultyPart}");
            Console.WriteLine($"Стоимость ремонта: {fixPrice} руб.");
            Console.WriteLine("\nВаши действия:");
            Console.WriteLine("1 - Взять заказ (если есть деталь на складе)");
            Console.WriteLine("2 - Отказать клиенту (штраф 300 руб.)");
            Console.WriteLine("3 - Закупить запчасти");
            Console.WriteLine("4 - Выйти из игры");
            string option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    HandleRepair(faultyPart, fixPrice, customerCount);
                    break;
                case "2":
                    DeclineCustomer(customerCount);
                    break;
                case "3":
                    OpenSupplyMenu();
                    break;
                case "4":
                    PersistSessionState();
                    Console.WriteLine($"Игра завершена! Итоговый баланс: {balance} руб.");
                    return;
                default:
                    Console.WriteLine("Неверный выбор! Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    continue;
            }

            customerCount++;
            Console.WriteLine("Нажмите любую клавишу для следующего клиента...");
            Console.ReadKey();
        }
    }

    private void HandleIncomingSupplies()
    {
        using (var context = new Rodionov7PRACTIKAEntities4())
        {
            // Получаем заказы, готовые к доставке
            var readyOrders = context.PurchaseOrders
                .Where(po => po.GameId == sessionId && po.DeliveryCounter <= 0)
                .ToList();

            foreach (var order in readyOrders)
            {
                if (stock.ContainsKey(order.PartName))
                    stock[order.PartName] += order.Quantity;
                else
                    stock[order.PartName] = order.Quantity;

                Console.WriteLine($"✓ Доставлены {order.Quantity} {order.PartName}");
                UpdateStockInDb(order.PartName, stock[order.PartName]);

                context.PurchaseOrders.Remove(order);
            }

            // Уменьшаем счетчик доставки для остальных заказов
            var pendingOrders = context.PurchaseOrders
                .Where(po => po.GameId == sessionId && po.DeliveryCounter > 0)
                .ToList();

            foreach (var order in pendingOrders)
                order.DeliveryCounter--;

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении заказов: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
            }
        }

        RefreshLocalSupplies();
    }

    private void DisplayCurrentStatus()
    {
        Console.WriteLine($"\nБаланс: {balance} руб.");
        Console.WriteLine("Склад:");
        if (stock.Count == 0)
        {
            Console.WriteLine(" (пусто)");
        }
        else
        {
            foreach (var item in stock)
                Console.WriteLine($" {item.Key}: {item.Value} шт.");
        }

        DisplayPendingSupplies();
    }

    private string PickRandomFault()
    {
        using (var context = new Rodionov7PRACTIKAEntities4())
        {
            var parts = context.Parts
                .Where(p => p.IsActive)
                .Select(p => p.Name)
                .ToList();

            if (parts.Count > 0)
            {
                int index = rng.Next(parts.Count);
                return parts[index];
            }
        }
        return "тормозные колодки"; // fallback
    }

    private int FetchPartCost(string partName)
    {
        using (var context = new Rodionov7PRACTIKAEntities4())
        {
            var part = context.Parts.FirstOrDefault(p => p.Name == partName);
            return part?.Price ?? 500;
        }
    }

    private void HandleRepair(string faultyPart, int fixPrice, int customerCount)
    {
        if (stock.ContainsKey(faultyPart) && stock[faultyPart] > 0)
        {
            stock[faultyPart]--;
            balance += fixPrice;
            UpdateStockInDb(faultyPart, stock[faultyPart]);
            PersistSessionState();
            RecordDeal(customerCount, faultyPart, fixPrice, "success");
            Console.WriteLine($"Успешный ремонт! +{fixPrice} руб.");
        }
        else
        {
            Console.WriteLine("Детали нет на складе...");
            if (stock.Count > 0)
            {
                string substitute = stock.Keys.First();
                stock[substitute]--;
                if (stock[substitute] == 0)
                    stock.Remove(substitute);
                int fine = fixPrice + 1000;
                balance -= fine;
                UpdateStockInDb(substitute, stock.ContainsKey(substitute) ? stock[substitute] : 0);
                PersistSessionState();
                RecordDeal(customerCount, faultyPart, -fine, "failed");
                Console.WriteLine($"Штраф: {fine} руб.");
            }
            else
            {
                int fine = fixPrice + 1500;
                balance -= fine;
                PersistSessionState();
                RecordDeal(customerCount, faultyPart, -fine, "no_parts");
                Console.WriteLine($"Штраф: {fine} руб.");
            }
        }
    }

    private void DeclineCustomer(int customerCount)
    {
        int fine = 300;
        balance -= fine;
        PersistSessionState();
        RecordDeal(customerCount, "refusal", -fine, "refused");
        Console.WriteLine($"Вы отказали клиенту. Штраф {fine} руб.");
    }

    private void OpenSupplyMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== ЗАКУПКА ЗАПЧАСТЕЙ ===");
            Console.WriteLine($"Баланс: {balance} руб.\n");

            var availableParts = FetchAvailableParts();
            int i = 1;
            foreach (var part in availableParts)
            {
                Console.WriteLine($"{i} - {part.Name}: {part.Price} руб./шт.");
                i++;
            }
            Console.WriteLine($"{i} - Вернуться к клиенту");
            Console.Write("\nВыбор: ");
            string input = Console.ReadLine();

            if (int.TryParse(input, out int sel))
            {
                if (sel == i) break;
                if (sel >= 1 && sel <= availableParts.Count)
                {
                    var chosen = availableParts[sel - 1];
                    Console.Write($"Сколько {chosen.Name} закупить? ");
                    if (int.TryParse(Console.ReadLine(), out int amount) && amount > 0)
                    {
                        int total = chosen.Price * amount;
                        if (total <= balance)
                        {
                            balance -= total;
                            PlaceSupplyOrder(chosen.Name, amount);
                            PersistSessionState();
                            Console.WriteLine($"Заказ оформлен! Списано {total} руб.");
                        }
                        else Console.WriteLine("Недостаточно средств!");
                    }
                }
            }
            Console.WriteLine("Нажмите любую клавишу...");
            Console.ReadKey();
        }
    }

    private List<Part> FetchAvailableParts()
    {
        using (var context = new Rodionov7PRACTIKAEntities4())
        {
            return context.Parts
                .Where(p => p.IsActive)
                .Select(p => new Part { Name = p.Name, Price = p.Price })
                .ToList();
        }
    }

    private void PlaceSupplyOrder(string partName, int amount)
    {
        using (var context = new Rodionov7PRACTIKAEntities4())
        {
            var order = new PurchaseOrders
            {
                GameId = sessionId,
                PartName = partName,
                Quantity = amount,
                DeliveryCounter = 2,
                OrderDate = DateTime.Now
            };
            context.PurchaseOrders.Add(order);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании заказа: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
            }
        }
        RefreshLocalSupplies();
    }

    private void UpdateStockInDb(string partName, int amount)
    {
        using (var context = new Rodionov7PRACTIKAEntities4())
        {
            var inventory = context.Inventory
                .FirstOrDefault(i => i.GameId == sessionId && i.PartName == partName);

            if (inventory != null)
                inventory.Quantity = amount;
            else
                context.Inventory.Add(new Inventory
                {
                    GameId = sessionId,
                    PartName = partName,
                    Quantity = amount
                });

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении склада: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
            }
        }
    }

    private void PersistSessionState()
    {
        using (var context = new Rodionov7PRACTIKAEntities4())
        {
            var session = context.GameSessions.FirstOrDefault(gs => gs.Id == sessionId);
            if (session != null)
            {
                session.CurrentMoney = balance;
                session.LastUpdate = DateTime.Now;

                try
                {
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при сохранении сессии: {ex.Message}");
                    if (ex.InnerException != null)
                        Console.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
                }
            }
        }
    }

    private void RecordDeal(int customerCount, string partName, int value, string outcome)
    {
        using (var context = new Rodionov7PRACTIKAEntities4())
        {
            var transaction = new Transactions
            {
                GameId = sessionId,
                ClientNumber = customerCount,
                PartName = partName,
                Amount = value,
                Status = outcome,
                TransactionDate = DateTime.Now
            };
            context.Transactions.Add(transaction);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"уйди отсю.да");
                
            }
        }
    }

    private void RefreshLocalSupplies()
    {
        suppliesInTransit.Clear();
        using (var context = new Rodionov7PRACTIKAEntities4())
        {
            var supplies = context.PurchaseOrders
                .Where(po => po.GameId == sessionId)
                .ToList();

            foreach (var supply in supplies)
            {
                suppliesInTransit.Add(new SupplyOrder(
                    supply.PartName,
                    supply.Quantity,
                    supply.DeliveryCounter
                ));
            }
        }
    }

    private void DisplayPendingSupplies()
    {
        if (suppliesInTransit.Count > 0)
        {
            Console.WriteLine("\nОжидаются поставки:");
            foreach (var s in suppliesInTransit)
                Console.WriteLine($" {s.PartName}: {s.Quantity} шт. (через {s.DeliveryCounter} клиентов)");
        }
    }
}

// Вспомогательные классы
class SupplyOrder
{
    public string PartName { get; set; }
    public int Quantity { get; set; }
    public int DeliveryCounter { get; set; }

    public SupplyOrder(string name, int qty, int counter)
    {
        PartName = name;
        Quantity = qty;
        DeliveryCounter = counter;
    }
}

class Part
{
    public string Name { get; set; }
    public int Price { get; set; }
}

// Точка входа
class Program
{
    static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("Добро пожаловать в автосервис!");
            var simulator = new CarRepairSimulator(5000);
            simulator.StartSimulation();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Критическая ошибка: {ex.Message}");
            Console.WriteLine("Проверьте подключение к базе данных");
            Console.WriteLine("Детали: " + ex.ToString());
            Console.ReadKey();
        }
    }
}

//1.Какие существенные сущности в задаче?
//Сущности:
//CarRepairSimulator-основной симулятор
//GameSession-игровая сессия
//Parts-запчасти
//PurchaseOrders-заказы на поставку
//Inventory-складские запасы
//Transactions-транзакции
//SupplyOrder-поставка (вспомогательный класс)

//2. Какие глаголы в задаче?
//Методы/Операции:
//HandleRepair()-обработать ремонт
//PlaceSupplyOrder()-разместить заказ
//DeclineCustomer()-отказать клиенту
//UpdateStockInDb()-обновить склад
//PersistSessionState()-сохранить состояние
//RecordDeal()-записать сделку
//HandleIncomingSupplies()-обработать поставки

//3. Какие данные ВСЕГДА вместе?
//Группировка в классы:
//sessionId + balance + stock → класс GameState
//PartName + Quantity + DeliveryCounter → класс SupplyOrder (уже есть)
//ClientNumber + PartName + Amount + Status → класс TransactionData

//5. Что ПОВТОРЯЕТСЯ в разных местах?
//Вынести в отдельные методы:

//Работа с БД-повтор using (var context = ...)
//одинаковые try-catch
//Обновление UI -вывод статуса и меню