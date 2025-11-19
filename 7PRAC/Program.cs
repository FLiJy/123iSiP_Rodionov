using System;
using System.Collections.Generic;
using System.Linq;
using _7PRAC; // оставьте ваш неймспейс с EDMX/EF-контекстом

class CarRepairSimulator
{
    private decimal balance;
    private Dictionary<string, int> stock; // локальный вид склада (Name -> Qty)
    private List<SupplyOrder> suppliesInTransit;
    private Random rng;
    private const int WarehouseId = 1;

    public CarRepairSimulator(decimal initialBalance)
    {
        balance = initialBalance;
        stock = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        suppliesInTransit = new List<SupplyOrder>();
        rng = new Random();
        StartNewSessionRandomInventory();
        RefreshLocalSuppliesFromDb(); // загрузим текущие поставки (в нашем случае нет таблицы заказов, но оставлю вызов для совместимости)
    }

    // Заполняет случайный инвентарь, записывает в WareHouseParts и баланс в WareHouse
    private void StartNewSessionRandomInventory()
    {
        using (var context = new Rodionov8PRACEntities1())
        {
            // Обновим/создадим запись склада с балансом
            var wh = context.WareHouse.FirstOrDefault(w => w.ID == WarehouseId);
            if (wh == null)
            {
                wh = new WareHouse { ID = WarehouseId, balance = balance };
                context.WareHouse.Add(wh);
            }
            else
            {
                wh.balance = balance;
            }

            // Для каждой детали в таблице Parts генерируем случайное кол-во (0..10) и записываем в WareHouseParts
            var parts = context.parts.ToList();
            foreach (var p in parts)
            {
                int qty = rng.Next(0, 11); // случайное количество 0..10
                stock[p.Name] = qty;

                var whPart = context.WareHouseParts.FirstOrDefault(wp => wp.SkladID == WarehouseId && wp.PartID == p.ID);
                if (whPart != null)
                {
                    whPart.Count = qty;
                }
                else
                {
                    context.WareHouseParts.Add(new WareHouseParts
                    {
                        SkladID = WarehouseId,
                        PartID = p.ID,
                        Count = qty
                    });
                }
            }

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при инициализации склада: " + ex.Message);
                if (ex.InnerException != null) Console.WriteLine(ex.InnerException.Message);
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
            HandleIncomingSupplies(); // обрабатываем доставленные поставки из памяти
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
                    PersistWarehouseState();
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

    // Поставки в пути хранятся в suppliesInTransit (в памяти). Этот метод доставляет те, у которых DeliveryCounter<=0
    private void HandleIncomingSupplies()
    {
        var delivered = suppliesInTransit.Where(s => s.DeliveryCounter <= 0).ToList();
        if (delivered.Count > 0)
        {
            using (var context = new Rodionov8PRACEntities1())
            {
                foreach (var order in delivered)
                {
                    // находим ID детали по имени
                    var part = context.parts.FirstOrDefault(p => p.Name == order.PartName);
                    if (part == null) continue;

                    // обновляем WareHouseParts
                    var whPart = context.WareHouseParts.FirstOrDefault(wp => wp.SkladID == WarehouseId && wp.PartID == part.ID);
                    if (whPart != null)
                        whPart.Count += order.Quantity;
                    else
                        context.WareHouseParts.Add(new WareHouseParts { SkladID = WarehouseId, PartID = part.ID, Count = order.Quantity });

                    // обновляем локально
                    if (stock.ContainsKey(order.PartName))
                        stock[order.PartName] += order.Quantity;
                    else
                        stock[order.PartName] = order.Quantity;

                    Console.WriteLine($"✓ Доставлены {order.Quantity} {order.PartName}");
                }

                try
                {
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при применении доставок: " + ex.Message);
                }
            }

            // удаляем доставленные из списка
            suppliesInTransit.RemoveAll(s => s.DeliveryCounter <= 0);
        }

        // уменьшаем счетчик для остальных заказов
        foreach (var s in suppliesInTransit)
            s.DeliveryCounter--;
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
        using (var context = new Rodionov8PRACEntities1())
        {
            var parts = context.parts.Select(p => p.Name).ToList();
            if (parts.Count > 0)
                return parts[rng.Next(parts.Count)];
        }
        return "Тормозные колодки";
    }

    private int FetchPartCost(string partName)
    {
        using (var context = new Rodionov8PRACEntities1())
        {
            var part = context.parts.FirstOrDefault(p => p.Name == partName);
            return part != null ? (int)part.Price : 500;
        }
    }

    private void HandleRepair(string faultyPart, int fixPrice, int clientNumber)
    {
        if (stock.ContainsKey(faultyPart) && stock[faultyPart] > 0)
        {
            stock[faultyPart]--;
            UpdateWareHousePartsQuantity(faultyPart, stock[faultyPart]);
            balance += fixPrice;
            PersistWarehouseState();
            Console.WriteLine($"Успешный ремонт! +{fixPrice} руб.");
        }
        else
        {
            Console.WriteLine("Детали нет на складе...");
            if (stock.Count > 0)
            {
                string substitute = stock.Keys.First();
                stock[substitute]--;
                UpdateWareHousePartsQuantity(substitute, stock[substitute]);
                int fine = fixPrice + 1000;
                balance -= fine;
                PersistWarehouseState();
                Console.WriteLine($"Использовано заменяющее: {substitute}. Штраф: {fine} руб.");
            }
            else
            {
                int fine = fixPrice + 1500;
                balance -= fine;
                PersistWarehouseState();
                Console.WriteLine($"Нет деталей вовсе. Штраф: {fine} руб.");
            }
        }
    }

    private void DeclineCustomer(int clientNumber)
    {
        int fine = 300;
        balance -= fine;
        PersistWarehouseState();
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
                            // Выставляем заказ: delivery через 2 клиента (храним в памяти)
                            suppliesInTransit.Add(new SupplyOrder(chosen.Name, amount, 2));
                            PersistWarehouseState(); // сохраняем только баланс в WareHouse
                            Console.WriteLine($"Заказ оформлен! Списано {total} руб. Поставка через 2 клиента.");
                        }
                        else Console.WriteLine("Недостаточно средств!");
                    }
                }
            }
            Console.WriteLine("Нажмите любую клавишу...");
            Console.ReadKey();
        }
    }

    private List<SimplePart> FetchAvailableParts()
    {
        using (var context = new Rodionov8PRACEntities1())
        {
            return context.parts
                .Select(p => new SimplePart { Name = p.Name, Price = (int)p.Price })
                .ToList();
        }
    }

    // Обновляет запись в WareHouseParts для данной детали (по имени)
    private void UpdateWareHousePartsQuantity(string partName, int amount)
    {
        using (var context = new Rodionov8PRACEntities1())
        {
            var part = context.parts.FirstOrDefault(p => p.Name == partName);
            if (part == null) return;

            var whPart = context.WareHouseParts.FirstOrDefault(wp => wp.SkladID == WarehouseId && wp.PartID == part.ID);
            if (whPart != null)
                whPart.Count = amount;
            else
                context.WareHouseParts.Add(new WareHouseParts { SkladID = WarehouseId, PartID = part.ID, Count = amount });

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при обновлении WareHouseParts: " + ex.Message);
            }
        }
    }

    // Сохраняет баланс склада (WareHouse)
    private void PersistWarehouseState()
    {
        using (var context = new Rodionov8PRACEntities1())
        {
            var wh = context.WareHouse.FirstOrDefault(w => w.ID == WarehouseId);
            if (wh == null)
            {
                context.WareHouse.Add(new WareHouse { ID = WarehouseId, balance = balance });
            }
            else
            {
                wh.balance = balance;
            }

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при сохранении баланса: " + ex.Message);
            }
        }
    }

    // Обновляет локальный кэш suppliesInTransit из БД — нет таблицы заказов, поэтому метод оставлен пустым для совместимости
    private void RefreshLocalSuppliesFromDb()
    {
        // Если у вас нет таблицы PurchaseOrders, то поставки в пути хранятся только в suppliesInTransit (память).
        // Если нужна персистентная очередь заказов, можно добавить таблицу и тут загрузить записи.
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

class SimplePart
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
            var simulator = new CarRepairSimulator(10000m); // начальный баланс можно поменять
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