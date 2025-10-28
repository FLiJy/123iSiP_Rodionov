using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

// Program.cs
// Скелет программы автосервиса. Все классы и методы объявлены, но не реализованы.
// Это первая стадия: декомпозиция + объявление API классов.
// В следующих шагах сюда будут добавлены реализации и тесты.

// Перечисления
public enum RepairStatus
{
    Pending,
    Completed,
    Declined,
    FailedReplacement
}

public enum TransactionType
{
    Income,
    Expense,
    Penalty
}

// Сущность: запчасть (справочник)
public class Part
{
    public int PartID { get; set; }
    public string PartCode { get; set; }           // например "BRK-001"
    public string PartName { get; set; }
    public decimal PurchasePrice { get; set; }    // цена закупки
    public decimal SalePrice { get; set; }        // цена продажи в ремонте

    // Валидация
    public bool Validate()
    {
        // проверяет PartName не пустой, цены >= 0, PartCode не пустой
        return default;
    }
}

// Склад — количество запчастей на складе
public class InventoryItem
{
    public int InventoryID { get; set; }
    public Part Part { get; set; }
    public int Quantity { get; set; }

    public bool Validate()
    {
        // Проверка Quantity >= 0 и Part != null
        return default;
    }
}

// Клиент
public class Client
{
    public int ClientID { get; set; }
    public string Name { get; set; }
    public string Contact { get; set; } // опционально

    // Создать клиента (валидация)
    public static Client Create(string name, string contact)
    {
        // Проверка входных данных
        return default;
    }
}

// Машина клиента
public class Car
{
    public int CarID { get; set; }
    public int ClientID { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public string Plate { get; set; }

    // Доп. методы если нужны
}

// Ремонт (заказ)
public class RepairJob
{
    public int RepairID { get; set; }
    public Client Client { get; set; }
    public Car Car { get; set; }
    public Part RequiredPart { get; set; } // какая деталь сломалась
    public decimal RepairPrice { get; set; } // цена детали + работа
    public decimal WorkFee { get; set; }
    public DateTime RepairDate { get; set; }
    public RepairStatus Status { get; set; }

    // Описание ремонта для вывода в UI
    public string Describe()
    {
        return default;
    }
}

// Заказ на закупку (поступит через N машин)
public class PurchaseOrder
{
    public int POID { get; set; }
    public Part Part { get; set; }
    public int Quantity { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MachinesUntilArrival { get; set; } // через сколько машин поступит
    public bool IsApplied { get; set; }

    // Валидация
    public bool Validate()
    {
        return default;
    }
}

// Транзакция (финансы)
public class Transaction
{
    public int TransactionID { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; } // положительное для дохода, отрицательное для расхода
    public TransactionType Type { get; set; }
    public string Description { get; set; }
}

// Менеджер склада
public class InventoryManager
{
    // Храним локальную копию склада (в реальной реализации синхронизируем с БД)
    public List<InventoryItem> Items { get; set; } = new List<InventoryItem>();

    public InventoryManager()
    {
    }

    // Проверяет, есть ли достаточное количество детали
    public bool HasPart(int partId, int requiredQty = 1)
    {
        return default;
    }

    // Уменьшает количество
    public void ConsumePart(int partId, int qty)
    {
    }

    // Добавляет количество (при поступлении заказа)
    public void AddPart(int partId, int qty)
    {
    }

    // Получить текущий список для UI
    public IEnumerable<InventoryItem> List()
    {
        return default;
    }
}

// Менеджер финансов
public class FinanceManager
{
    public decimal Balance { get; set; }

    public FinanceManager(decimal startingBalance)
    {
        Balance = startingBalance;
    }

    public void ApplyTransaction(Transaction t)
    {
    }

    public void AddIncome(decimal amount, string description)
    {
    }

    public void AddExpense(decimal amount, string description)
    {
    }
}

// Менеджер заказов/ремонтов — логика обслуживания клиента
public class RepairManager
{
    private InventoryManager _inventory;
    private FinanceManager _finance;
    private List<PurchaseOrder> _pendingOrders; // локально

    // Счётчик машин, нужен для отсчёта поставок (каждый приезд машины уменьшает MachinesUntilArrival)
    private int _machinesProcessedSinceStart = 0;

    public RepairManager(InventoryManager inventory, FinanceManager finance)
    {
        _inventory = inventory;
        _finance = finance;
        _pendingOrders = new List<PurchaseOrder>();
    }

    // Обработка клиента: принимает RepairJob, возвращает результат (true если успешно)
    public bool ProcessClient(RepairJob job)
    {
        return default;
    }

    // Отказ клиента — штраф
    public void DeclineClient(RepairJob job)
    {
    }

    // Создать заказ на покупку
    public PurchaseOrder PlacePurchaseOrder(int partId, int quantity)
    {
        return default;
    }

    // Вызывается при каждом новом клиенте — уменьшает счетчик MachinesUntilArrival и применяет поступившие заказы
    public void OnNewClientArrived()
    {
    }

    // Применить все заказы, которые пришли (перенести в склад)
    public void ApplyArrivedOrders()
    {
    }
}

// Класс для работы с базой данных MS SQL Server.
// Обертка над SqlConnection/SqlCommand для CRUD операций.
// ВАЖНО: Вставьте вашу строку подключения в connectionString.
public class Database
{
    private string _connectionString;

    public Database(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Открывает соединение и возвращает SqlConnection (в реальной версии используем using)
    public SqlConnection GetConnection()
    {
        return default;
    }

    // Методы загрузки/сохранения справочников и состояний
    public List<Part> LoadParts()
    {
        return default;
    }

    public void SavePart(Part p)
    {
    }

    public List<InventoryItem> LoadInventory()
    {
        return default;
    }

    public void SaveInventoryItem(InventoryItem item)
    {
    }

    public List<RepairJob> LoadRepairs()
    {
        return default;
    }

    public void SaveRepair(RepairJob r)
    {
    }

    public void SaveTransaction(Transaction t)
    {
    }

    public void SavePurchaseOrder(PurchaseOrder po)
    {
    }

    public List<PurchaseOrder> LoadPendingPurchaseOrders()
    {
        return default;
    }
}

// Консольный UI — меню и ввод пользователя
public static class ConsoleUI
{
    public static void ShowMainMenu()
    {
    }

    public static string ReadNonEmptyString(string prompt)
    {
        return default;
    }

    public static int ReadInt(string prompt, int min, int max)
    {
        return default;
    }

    public static decimal ReadDecimal(string prompt, decimal min)
    {
        return default;
    }
}

// Главный класс программы
public class Program
{
    // Строка подключения. Замените {YOUR_CONN_STRING} на реальную строку.
    // Пример: "Server=YOUR_SERVER;Database=AutoServiceDB;Trusted_Connection=True;"
    private const string ConnectionStringPlaceholder = "{YOUR_CONN_STRING}";

    public static void Main(string[] args)
    {
        // Здесь мы будем: 1) инициализировать DB, 2) загрузить справочники/склад, 3) показать меню и обрабатывать клиентов.
        // На данный момент метод пуст — это точка старта для следующего этапа.
    }
}
