using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

class CarRepairSimulator
{
    private int balance;
    private Dictionary<string, int> stock;
    private List<SupplyOrder> suppliesInTransit;
    private Random rng;
    private string dbConnection;
    private int sessionId;

    public CarRepairSimulator(int initialBalance)
    {
        dbConnection = $"Server=bd-kip.fa.ru;Database=Rodionov7PRACTIKA;User Id=sa;Password=1qaz!QAZ;";
        balance = initialBalance;
        stock = new Dictionary<string, int>();
        suppliesInTransit = new List<SupplyOrder>();
        rng = new Random();
        StartNewSession();
    }

    private void StartNewSession()
    {
        using (var connection = new SqlConnection(dbConnection))
        {
            connection.Open();
            // Создаем новую игровую сессию
            string sql = @"INSERT INTO GameSessions (StartMoney, CurrentMoney, CreatedDate, LastUpdate)
                          OUTPUT INSERTED.Id
                          VALUES (@StartMoney, @CurrentMoney, GETDATE(), GETDATE())";
            using (var cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@StartMoney", balance);
                cmd.Parameters.AddWithValue("@CurrentMoney", balance);
                sessionId = (int)cmd.ExecuteScalar();
            }
            // Загружаем начальные детали из таблицы Parts
            sql = "SELECT Name, InitialQuantity FROM Parts WHERE IsActive = 1 AND InitialQuantity > 0";
            using (var cmd = new SqlCommand(sql, connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string partName = reader["Name"].ToString();
                    int initialQuantity = (int)reader["InitialQuantity"];
                    stock[partName] = initialQuantity;
                    UpdateStockInDb(partName, initialQuantity);
                }
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
        using (var connection = new SqlConnection(dbConnection))
        {
            connection.Open();
            // Получаем заказы, готовые к доставке
            string sql = "SELECT Id, PartName, Quantity FROM PurchaseOrders WHERE GameId = @GameId AND DeliveryCounter <= 0";
            using (var cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@GameId", sessionId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string partName = reader["PartName"].ToString();
                        int quantity = (int)reader["Quantity"];
                        int orderId = (int)reader["Id"];
                        if (stock.ContainsKey(partName))
                            stock[partName] += quantity;
                        else
                            stock[partName] = quantity;
                        Console.WriteLine($"✓ Доставлены {quantity} {partName}");
                        UpdateStockInDb(partName, stock[partName]);
                        RemoveSupplyOrder(orderId);
                    }
                }
            }
            // Уменьшаем счетчик доставки для остальных заказов
            sql = "UPDATE PurchaseOrders SET DeliveryCounter = DeliveryCounter - 1 WHERE GameId = @GameId AND DeliveryCounter > 0";
            using (var cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@GameId", sessionId);
                cmd.ExecuteNonQuery();
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
            {
                Console.WriteLine($" {item.Key}: {item.Value} шт.");
            }
        }
        DisplayPendingSupplies();
    }

    private string PickRandomFault()
    {
        using (var connection = new SqlConnection(dbConnection))
        {
            connection.Open();
            string sql = "SELECT Name FROM Parts WHERE IsActive = 1";
            using (var cmd = new SqlCommand(sql, connection))
            using (var reader = cmd.ExecuteReader())
            {
                List<string> parts = new List<string>();
                while (reader.Read())
                {
                    parts.Add(reader["Name"].ToString());
                }
                if (parts.Count > 0)
                {
                    int index = rng.Next(parts.Count);
                    return parts[index];
                }
            }
        }
        return "тормозные колодки"; // Default fallback
    }

    private int FetchPartCost(string partName)
    {
        using (var connection = new SqlConnection(dbConnection))
        {
            connection.Open();
            string sql = "SELECT Price FROM Parts WHERE Name = @Name";
            using (var cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@Name", partName);
                var result = cmd.ExecuteScalar();
                return result != null ? (int)result : 500;
            }
        }
    }

    private void HandleRepair(string faultyPart, int fixPrice, int customerCount)
    {
        if (stock.ContainsKey(faultyPart) && stock[faultyPart] > 0)
        {
            // Successful fix
            stock[faultyPart]--;
            balance += fixPrice;
            UpdateStockInDb(faultyPart, stock[faultyPart]);
            PersistSessionState();
            RecordDeal(customerCount, faultyPart, fixPrice, "success");
            Console.WriteLine($"Успешный ремонт! Вы заработали {fixPrice} руб.");
        }
        else
        {
            // Failed attempt
            Console.WriteLine("Нужной детали нет на складе! Производится замена случайной деталью...");
            if (stock.Count > 0)
            {
                string substitutePart = stock.Keys.First();
                stock[substitutePart]--;
                if (stock[substitutePart] == 0)
                    stock.Remove(substitutePart);
                int fine = fixPrice + 1000;
                balance -= fine;
                UpdateStockInDb(substitutePart, stock.ContainsKey(substitutePart) ? stock[substitutePart] : 0);
                PersistSessionState();
                RecordDeal(customerCount, faultyPart, -fine, "failed");
                Console.WriteLine($"Клиент недоволен! Штраф: {fine} руб.");
            }
            else
            {
                int fine = fixPrice + 1500;
                balance -= fine;
                PersistSessionState();
                RecordDeal(customerCount, faultyPart, -fine, "no_parts");
                Console.WriteLine($"На складе нет деталей! Штраф: {fine} руб.");
            }
        }
    }

    private void DeclineCustomer(int customerCount)
    {
        int fine = 300;
        balance -= fine;
        PersistSessionState();
        RecordDeal(customerCount, "refusal", -fine, "refused");
        Console.WriteLine($"Вы отказали клиенту. Штраф: {fine} руб.");
    }

    private void OpenSupplyMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== ЗАКУПКА ЗАПЧАСТЕЙ ===");
            Console.WriteLine($"Баланс: {balance} руб.");
            Console.WriteLine("\nДоступные запчасти:");
            var availableParts = FetchAvailableParts();
            int index = 1;
            foreach (var part in availableParts)
            {
                Console.WriteLine($"{index} - {part.Name}: {part.Price} руб./шт.");
                index++;
            }
            Console.WriteLine($"{index} - Вернуться к клиенту");
            Console.Write("\nВыберите деталь для заказа: ");
            string input = Console.ReadLine();
            if (int.TryParse(input, out int selection))
            {
                if (selection == index)
                    break;
                if (selection >= 1 && selection <= availableParts.Count)
                {
                    var chosenPart = availableParts[selection - 1];
                    Console.Write($"Сколько {chosenPart.Name} закупить? ");
                    if (int.TryParse(Console.ReadLine(), out int amount) && amount > 0)
                    {
                        int total = chosenPart.Price * amount;
                        if (total <= balance)
                        {
                            balance -= total;
                            PlaceSupplyOrder(chosenPart.Name, amount);
                            PersistSessionState();
                            Console.WriteLine($"Заказ на {amount} {chosenPart.Name} оформлен! Доставка через 2 клиента.");
                            Console.WriteLine($"Списано: {total} руб.");
                        }
                        else
                        {
                            Console.WriteLine("Недостаточно денег!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Неверное количество!");
                    }
                }
                else
                {
                    Console.WriteLine("Неверный выбор!");
                }
            }
            else
            {
                Console.WriteLine("Неверный ввод!");
            }
            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }
    }

    private List<Part> FetchAvailableParts()
    {
        var partsList = new List<Part>();
        using (var connection = new SqlConnection(dbConnection))
        {
            connection.Open();
            string sql = "SELECT Name, Price FROM Parts WHERE IsActive = 1";
            using (var cmd = new SqlCommand(sql, connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    partsList.Add(new Part
                    {
                        Name = reader["Name"].ToString(),
                        Price = (int)reader["Price"]
                    });
                }
            }
        }
        return partsList;
    }

    private void PlaceSupplyOrder(string partName, int amount)
    {
        using (var connection = new SqlConnection(dbConnection))
        {
            connection.Open();
            string sql = @"INSERT INTO PurchaseOrders (GameId, PartName, Quantity, DeliveryCounter, OrderDate)
                          VALUES (@GameId, @PartName, @Quantity, 2, GETDATE())";
            using (var cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@GameId", sessionId);
                cmd.Parameters.AddWithValue("@PartName", partName);
                cmd.Parameters.AddWithValue("@Quantity", amount);
                cmd.ExecuteNonQuery();
            }
        }
        RefreshLocalSupplies();
    }

    private void UpdateStockInDb(string partName, int amount)
    {
        using (var connection = new SqlConnection(dbConnection))
        {
            connection.Open();
            string checkSql = "SELECT COUNT(*) FROM Inventory WHERE GameId = @GameId AND PartName = @PartName";
            using (var checkCmd = new SqlCommand(checkSql, connection))
            {
                checkCmd.Parameters.AddWithValue("@GameId", sessionId);
                checkCmd.Parameters.AddWithValue("@PartName", partName);
                int exists = (int)checkCmd.ExecuteScalar();
                if (exists > 0)
                {
                    string updateSql = "UPDATE Inventory SET Quantity = @Quantity WHERE GameId = @GameId AND PartName = @PartName";
                    using (var updateCmd = new SqlCommand(updateSql, connection))
                    {
                        updateCmd.Parameters.AddWithValue("@Quantity", amount);
                        updateCmd.Parameters.AddWithValue("@GameId", sessionId);
                        updateCmd.Parameters.AddWithValue("@PartName", partName);
                        updateCmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    string insertSql = "INSERT INTO Inventory (GameId, PartName, Quantity) VALUES (@GameId, @PartName, @Quantity)";
                    using (var insertCmd = new SqlCommand(insertSql, connection))
                    {
                        insertCmd.Parameters.AddWithValue("@GameId", sessionId);
                        insertCmd.Parameters.AddWithValue("@PartName", partName);
                        insertCmd.Parameters.AddWithValue("@Quantity", amount);
                        insertCmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }

    private void PersistSessionState()
    {
        using (var connection = new SqlConnection(dbConnection))
        {
            connection.Open();
            string sql = "UPDATE GameSessions SET CurrentMoney = @CurrentMoney, LastUpdate = GETDATE() WHERE Id = @Id";
            using (var cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@CurrentMoney", balance);
                cmd.Parameters.AddWithValue("@Id", sessionId);
                cmd.ExecuteNonQuery();
            }
        }
    }

    private void RecordDeal(int customerCount, string partName, int value, string outcome)
    {
        using (var connection = new SqlConnection(dbConnection))
        {
            connection.Open();
            string sql = @"INSERT INTO Transactions (GameId, ClientNumber, PartName, Amount, Status, TransactionDate)
                          VALUES (@GameId, @ClientNumber, @PartName, @Amount, @Status, GETDATE())";
            using (var cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@GameId", sessionId);
                cmd.Parameters.AddWithValue("@ClientNumber", customerCount);
                cmd.Parameters.AddWithValue("@PartName", partName);
                cmd.Parameters.AddWithValue("@Amount", value);
                cmd.Parameters.AddWithValue("@Status", outcome);
                cmd.ExecuteNonQuery();
            }
        }
    }

    private void RefreshLocalSupplies()
    {
        suppliesInTransit.Clear();
        using (var connection = new SqlConnection(dbConnection))
        {
            connection.Open();
            string sql = "SELECT PartName, Quantity, DeliveryCounter FROM PurchaseOrders WHERE GameId = @GameId";
            using (var cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@GameId", sessionId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        suppliesInTransit.Add(new SupplyOrder(
                            reader["PartName"].ToString(),
                            (int)reader["Quantity"],
                            (int)reader["DeliveryCounter"]
                        ));
                    }
                }
            }
        }
    }

    private void DisplayPendingSupplies()
    {
        if (suppliesInTransit.Count > 0)
        {
            Console.WriteLine("\nОжидаются поставки:");
            foreach (var supply in suppliesInTransit)
            {
                Console.WriteLine($" {supply.PartName}: {supply.Quantity} шт. (через {supply.DeliveryCounter} клиентов)");
            }
        }
    }

    private void RemoveSupplyOrder(int orderId)
    {
        using (var connection = new SqlConnection(dbConnection))
        {
            connection.Open();
            string sql = "DELETE FROM PurchaseOrders WHERE Id = @Id";
            using (var cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@Id", orderId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}

class SupplyOrder
{
    public string PartName { get; set; }
    public int Quantity { get; set; }
    public int DeliveryCounter { get; set; }

    public SupplyOrder(string partName, int quantity, int deliveryCounter)
    {
        PartName = partName;
        Quantity = quantity;
        DeliveryCounter = deliveryCounter;
    }
}

class Part
{
    public string Name { get; set; }
    public int Price { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("Добро пожаловать в автосервис!");
            CarRepairSimulator simulator = new CarRepairSimulator(5000);
            simulator.StartSimulation();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            Console.WriteLine("Проверьте подключение к базе данных SQL Server и правильность логина/пароля");
        }
    }
}