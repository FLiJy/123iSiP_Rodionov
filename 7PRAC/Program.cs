using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7PRAC
{
    internal class Program
    {
        static void Main(string[] args)
        {
        }
    }
}

// Program.cs — весь проект в одном файле
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceSingleFile
{
    // ===== ENUMS =====
    public enum PartCategory { Engine, Transmission, Electrical, Brake, Suspension, Body, Other }
    public enum RepairStatus { Pending, InProgress, Completed, Declined, Failed }
    public enum TransactionType { Income, Expense, Penalty, Purchase }

    // ===== MODELS / ENTITIES =====
    public class Client
    {
        public int Id { get; set; }
        [Required] public string FullName { get; set; }
        [Required] public string ContactPhone { get; set; }
        public string Email { get; set; }
        public List<Vehicle> Vehicles { get; set; } = new();
    }

    public class Vehicle
    {
        public int Id { get; set; }
        [Required] public string LicensePlate { get; set; }
        public string Model { get; set; }
        public int OwnerId { get; set; }
        public Client Owner { get; set; }
    }

    public class Part
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; }
        public PartCategory Category { get; set; }
        [Range(0, double.MaxValue)] public decimal PurchasePrice { get; set; }
        [Range(0, double.MaxValue)] public decimal SalePrice { get; set; }
        public string SupplierCode { get; set; }
    }

    public class InventoryItem
    {
        public int Id { get; set; }
        public int PartId { get; set; }
        public Part Part { get; set; }
        [Range(0, int.MaxValue)] public int Quantity { get; set; }
        [Range(0, int.MaxValue)] public int Reserved { get; set; }
    }

    public class RepairOrder
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public int RequiredPartId { get; set; }
        public Part RequiredPart { get; set; }
        [Range(0, double.MaxValue)] public decimal LaborCost { get; set; }
        [Range(0, double.MaxValue)] public decimal TotalCost { get; set; }
        public RepairStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Note { get; set; }
    }

    public class Supplier
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; }
        public string Contact { get; set; }
    }

    public class PurchaseOrder
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }
        public int RemainingClientCounter { get; set; } // через сколько клиентов придёт поставка
        public bool IsDelivered { get; set; } = false;
        public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
        public List<PurchaseOrderItem> Items { get; set; } = new();
    }

    public class PurchaseOrderItem
    {
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public PurchaseOrder PurchaseOrder { get; set; }
        public int PartId { get; set; }
        public Part Part { get; set; }
        [Range(1, int.MaxValue)] public int Quantity { get; set; }
        [Range(0, double.MaxValue)] public decimal UnitPrice { get; set; }
    }

    public class Transaction
    {
        public int Id { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; } // positive absolute amount
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public string Description { get; set; }
    }

    // ===== DB CONTEXT =====
    public class AutoServiceDbContext : DbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Part> Parts { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<RepairOrder> RepairOrders { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        private readonly string _conn;

        public AutoServiceDbContext(string connectionString)
        {
            _conn = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_conn);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Part>().HasIndex(p => p.Name);
            modelBuilder.Entity<InventoryItem>().HasOne(i => i.Part).WithMany().HasForeignKey(i => i.PartId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Vehicle>().HasOne(v => v.Owner).WithMany(c => c.Vehicles).HasForeignKey(v => v.OwnerId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PurchaseOrderItem>().HasOne(i => i.Part).WithMany().HasForeignKey(i => i.PartId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    // ===== SERVICE: бизнес-логика =====
    public class AutoServiceService
    {
        private readonly AutoServiceDbContext _db;
        public AutoServiceService(AutoServiceDbContext db) { _db = db; }

        // Баланс: доходы - (покупки + штрафы + расходы)
        public decimal GetBalance()
        {
            var all = _db.Transactions.ToList();
            decimal sum = 0m;
            foreach (var t in all)
            {
                if (t.Type == TransactionType.Income) sum += t.Amount;
                else sum -= t.Amount; // Purchase, Penalty, Expense -> уменьшают баланс
            }
            return sum;
        }

        private void AddTransaction(Transaction tx)
        {
            if (tx == null) throw new ArgumentNullException(nameof(tx));
            _db.Transactions.Add(tx);
            _db.SaveChanges();
        }

        // Создать заказ на покупку: деньги списываются сразу (как Purchase), поставка придет через deliveryClients
        public PurchaseOrder CreatePurchaseOrder(int supplierId, List<PurchaseOrderItem> items, int deliveryClients = 2)
        {
            if (items == null || items.Count == 0) throw new ArgumentException("Items required");
            if (deliveryClients < 0) deliveryClients = 2;

            decimal total = items.Sum(i => i.UnitPrice * i.Quantity);
            if (total <= 0) throw new ArgumentException("Total must be positive");

            decimal balance = GetBalance();
            if (balance < total) throw new InvalidOperationException("Недостаточно средств для покупки");

            var po = new PurchaseOrder
            {
                SupplierId = supplierId,
                RemainingClientCounter = deliveryClients,
                IsDelivered = false
            };
            foreach (var it in items)
            {
                if (it.Quantity <= 0) throw new ArgumentException("Quantity must be > 0");
                po.Items.Add(new PurchaseOrderItem { PartId = it.PartId, Quantity = it.Quantity, UnitPrice = it.UnitPrice });
            }

            _db.PurchaseOrders.Add(po);
            // списываем деньги
            AddTransaction(new Transaction
            {
                Type = TransactionType.Purchase,
                Amount = total,
                Description = $"PurchaseOrder placed (supplier {supplierId})"
            });

            _db.SaveChanges();
            return po;
        }

        // Вызывать при приходе нового клиента: уменьшаем RemainingClientCounter и доставляем, если 0
        public void ProcessPurchaseDeliveriesOnNewClient()
        {
            var pending = _db.PurchaseOrders.Include(p => p.Items).Where(p => !p.IsDelivered).ToList();
            foreach (var po in pending)
            {
                po.RemainingClientCounter = Math.Max(0, po.RemainingClientCounter - 1);
                if (po.RemainingClientCounter == 0)
                {
                    foreach (var it in po.Items)
                    {
                        var inv = _db.InventoryItems.SingleOrDefault(i => i.PartId == it.PartId);
                        if (inv == null)
                        {
                            inv = new InventoryItem { PartId = it.PartId, Quantity = it.Quantity, Reserved = 0 };
                            _db.InventoryItems.Add(inv);
                        }
                        else
                        {
                            inv.Quantity += it.Quantity;
                        }
                    }
                    po.IsDelivered = true;
                    AddTransaction(new Transaction
                    {
                        Type = TransactionType.Income,
                        Amount = 0m,
                        Description = $"PurchaseOrder {po.Id} delivered (inventory updated)"
                    });
                }
            }
            _db.SaveChanges();
        }

        // Принять клиента — создаём RepairOrder (без выполнения)
        public RepairOrder ReceiveClient(int clientId, int vehicleId, int brokenPartId, decimal laborCost)
        {
            var client = _db.Clients.Find(clientId) ?? throw new ArgumentException("Client not found");
            var vehicle = _db.Vehicles.Find(vehicleId) ?? throw new ArgumentException("Vehicle not found");
            var part = _db.Parts.Find(brokenPartId) ?? throw new ArgumentException("Part not found");
            if (laborCost < 0) throw new ArgumentException("LaborCost must be >= 0");

            var order = new RepairOrder
            {
                ClientId = clientId,
                VehicleId = vehicleId,
                RequiredPartId = brokenPartId,
                LaborCost = laborCost,
                TotalCost = part.SalePrice + laborCost,
                Status = RepairStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            _db.RepairOrders.Add(order);
            _db.SaveChanges();
            return order;
        }

        // Попытка ремонта:
        // acceptIfMissingReplaceWithRandom:
        //   false — если нет детали, считаем отказом (штраф)
        //   true  — если нет детали, заменяем случайной существующей (если есть) => клиент недоволен => компенсация
        public bool AttemptRepair(int repairOrderId, bool acceptIfMissingReplaceWithRandom = false)
        {
            var order = _db.RepairOrders.Include(r => r.RequiredPart).FirstOrDefault(r => r.Id == repairOrderId)
                        ?? throw new ArgumentException("Order not found");

            if (order.Status != RepairStatus.Pending) throw new InvalidOperationException("Order in wrong status");

            // проверяем склад
            var neededInv = _db.InventoryItems.Include(i => i.Part)
                .FirstOrDefault(i => i.PartId == order.RequiredPartId && (i.Quantity - i.Reserved) > 0);

            if (neededInv != null)
            {
                // используем нужную деталь
                neededInv.Quantity -= 1;
                order.Status = RepairStatus.Completed;
                AddTransaction(new Transaction
                {
                    Type = TransactionType.Income,
                    Amount = order.TotalCost,
                    Description = $"Repair {order.Id} completed (part {order.RequiredPart.Name})"
                });
                _db.SaveChanges();
                return true;
            }
            else
            {
                if (!acceptIfMissingReplaceWithRandom)
                {
                    // отказ — плата за отказ
                    order.Status = RepairStatus.Declined;
                    decimal penalty = CalculateRefusalPenalty(order);
                    AddTransaction(new Transaction
                    {
                        Type = TransactionType.Penalty,
                        Amount = penalty,
                        Description = $"Service declined for order {order.Id}"
                    });
                    _db.SaveChanges();
                    return false;
                }
                else
                {
                    // попытка заменить на случайную существующую деталь
                    var available = _db.InventoryItems.Include(i => i.Part).Where(i => (i.Quantity - i.Reserved) > 0).ToList();
                    if (!available.Any())
                    {
                        order.Status = RepairStatus.Declined;
                        decimal penaltyNoParts = CalculateRefusalPenalty(order);
                        AddTransaction(new Transaction
                        {
                            Type = TransactionType.Penalty,
                            Amount = penaltyNoParts,
                            Description = $"Service declined — no parts available for order {order.Id}"
                        });
                        _db.SaveChanges();
                        return false;
                    }
                    var rnd = new Random();
                    var chosen = available[rnd.Next(available.Count)];
                    chosen.Quantity -= 1;
                    order.Status = RepairStatus.Failed; // клиент недоволен
                    decimal compensation = CalculateCompensation(order);
                    AddTransaction(new Transaction
                    {
                        Type = TransactionType.Penalty,
                        Amount = compensation,
                        Description = $"Wrong part used (used {chosen.Part.Name}) for order {order.Id}"
                    });
                    _db.SaveChanges();
                    return false;
                }
            }
        }

        private decimal CalculateRefusalPenalty(RepairOrder order)
        {
            return Math.Round(order.TotalCost * 0.2m + 50m, 2);
        }

        private decimal CalculateCompensation(RepairOrder order)
        {
            return Math.Round(order.TotalCost * 1.5m + 100m, 2);
        }

        // Утилиты для UI
        public List<InventoryItem> GetInventory() => _db.InventoryItems.Include(i => i.Part).ToList();
        public List<Part> GetParts() => _db.Parts.ToList();
        public List<RepairOrder> GetPendingOrders() => _db.RepairOrders.Where(r => r.Status == RepairStatus.Pending).ToList();
        public List<PurchaseOrder> GetPendingPurchaseOrders() => _db.PurchaseOrders.Include(p => p.Items).Where(p => !p.IsDelivered).ToList();
    }

    // ===== PROGRAM (Console UI) =====
    class Program
    {
        static void Main(string[] args)
        {
            const string conn = "Data Source=autoservice.db";
            using var db = new AutoServiceDbContext(conn);
            // Для одного файла удобнее EnsureCreated, чтобы не возиться с миграциями
            db.Database.EnsureCreated();

            SeedDataIfEmpty(db);

            var service = new AutoServiceService(db);

            Console.WriteLine("=== Симулятор автосервиса (всё в одном файле) ===");
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine();
                Console.WriteLine($"Баланс: {service.GetBalance():0.00}");
                Console.WriteLine("1) Принять следующего клиента");
                Console.WriteLine("2) Купить детали (создать заказ на поставку)");
                Console.WriteLine("3) Показать склад");
                Console.WriteLine("4) Показать ожидающие поставки");
                Console.WriteLine("5) Показать транзакции");
                Console.WriteLine("6) Выход");
                Console.Write("Выберите действие: ");
                var cmd = Console.ReadLine()?.Trim();

                try
                {
                    switch (cmd)
                    {
                        case "1":
                            HandleNextClient(db, service);
                            break;
                        case "2":
                            HandleBuyParts(db, service);
                            break;
                        case "3":
                            ShowInventory(service);
                            break;
                        case "4":
                            ShowPendingPO(service);
                            break;
                        case "5":
                            ShowTransactions(db);
                            break;
                        case "6":
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("Неверная команда");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка: " + ex.Message);
                }
            }

            Console.WriteLine("Игра завершена. Пока!");
        }

        static void SeedDataIfEmpty(AutoServiceDbContext db)
        {
            if (!db.Parts.Any())
            {
                var p1 = new Part { Name = "Тормозные колодки", Category = PartCategory.Brake, PurchasePrice = 200m, SalePrice = 400m };
                var p2 = new Part { Name = "Свеча зажигания", Category = PartCategory.Electrical, PurchasePrice = 50m, SalePrice = 150m };
                var p3 = new Part { Name = "Фильтр масляный", Category = PartCategory.Engine, PurchasePrice = 80m, SalePrice = 200m };
                db.Parts.AddRange(p1, p2, p3);
                db.SaveChanges();

                db.InventoryItems.Add(new InventoryItem { PartId = p1.Id, Quantity = 3, Reserved = 0 });
                db.InventoryItems.Add(new InventoryItem { PartId = p2.Id, Quantity = 5, Reserved = 0 });
                db.InventoryItems.Add(new InventoryItem { PartId = p3.Id, Quantity = 1, Reserved = 0 });

                db.Clients.Add(new Client { FullName = "Иван Иванов", ContactPhone = "+70000000001", Email = "ivan@example.com" });
                db.Clients.Add(new Client { FullName = "Петр Петров", ContactPhone = "+70000000002", Email = "petr@example.com" });
                db.SaveChanges();

                var c1 = db.Clients.First();
                var c2 = db.Clients.Skip(1).First();
                db.Vehicles.Add(new Vehicle { OwnerId = c1.Id, LicensePlate = "A111AA", Model = "Lada" });
                db.Vehicles.Add(new Vehicle { OwnerId = c2.Id, LicensePlate = "B222BB", Model = "Toyota" });

                db.Transactions.Add(new Transaction { Type = TransactionType.Income, Amount = 2000m, Description = "Start balance" });

                db.SaveChanges();
            }
        }

        static void HandleNextClient(AutoServiceDbContext db, AutoServiceService service)
        {
            // перед обработкой нового клиента — уменьшаем счетчики поставок и доставляем, если нужно
            service.ProcessPurchaseDeliveriesOnNewClient();

            // простая логика: берем случайного клиента и случайную поломку
            var client = db.Clients.Include(c => c.Vehicles).First();
            var vehicle = db.Vehicles.First(v => v.OwnerId == client.Id);
            var parts = db.Parts.ToList();
            var rnd = new Random();
            var brokenPart = parts[rnd.Next(parts.Count)];

            Console.WriteLine($"Клиент: {client.FullName}, авто: {vehicle.Model} ({vehicle.LicensePlate})");
            Console.WriteLine($"Сломалась деталь: {brokenPart.Name}. Цена детали: {brokenPart.SalePrice:0.00}");
            decimal labor = Math.Round((decimal)rnd.Next(100, 501), 2);
            Console.WriteLine($"Оплата за работу: {labor:0.00}. Итого клиент готов заплатить: {brokenPart.SalePrice + labor:0.00}");

            var order = service.ReceiveClient(client.Id, vehicle.Id, brokenPart.Id, labor);

            Console.Write("Принимаем заказ? (y = принять / n = отказ / a = принять даже если нет нужной детали): ");
            var answer = Console.ReadLine()?.Trim().ToLower();
            if (answer == "n")
            {
                service.AttemptRepair(order.Id, acceptIfMissingReplaceWithRandom: false);
                Console.WriteLine("Вы отказали клиенту (штраф).");
                return;
            }
            if (answer == "a")
            {
                var ok = service.AttemptRepair(order.Id, acceptIfMissingReplaceWithRandom: true);
                if (ok) Console.WriteLine("Ремонт успешно выполнен.");
                else Console.WriteLine("Ремонт завершился неправильно — удержана компенсация.");
                return;
            }
            // по умолчанию — принять, но не соглашаться на замену другой деталью
            var success = service.AttemptRepair(order.Id, acceptIfMissingReplaceWithRandom: false);
            if (success) Console.WriteLine("Ремонт успешно выполнен.");
            else Console.WriteLine("Нужной детали нет — отказ (штраф).");
        }

        static void HandleBuyParts(AutoServiceDbContext db, AutoServiceService service)
        {
            var parts = db.Parts.ToList();
            Console.WriteLine("Доступные детали:");
            foreach (var p in parts) Console.WriteLine($"{p.Id}) {p.Name} — цена закупки {p.PurchasePrice:0.00}");

            Console.Write("Введите id детали для покупки: ");
            if (!int.TryParse(Console.ReadLine(), out int partId)) { Console.WriteLine("Неверный id"); return; }
            var part = db.Parts.Find(partId);
            if (part == null) { Console.WriteLine("Не найдено"); return; }

            Console.Write("Введите количество: ");
            if (!int.TryParse(Console.ReadLine(), out int qty) || qty <= 0) { Console.WriteLine("Неверное количество"); return; }

            // выбираем/создаём поставщика-заглушку
            var supplier = db.Suppliers.FirstOrDefault();
            if (supplier == null)
            {
                supplier = new Supplier { Name = "Default Supplier", Contact = "supplier@example.com" };
                db.Suppliers.Add(supplier);
                db.SaveChanges();
            }

            var po = service.CreatePurchaseOrder(supplier.Id, new List<PurchaseOrderItem> {
                new PurchaseOrderItem { PartId = partId, Quantity = qty, UnitPrice = part.PurchasePrice }
            }, deliveryClients: 2);

            Console.WriteLine($"Заказ создан (PO {po.Id}). Поставка придет через {po.RemainingClientCounter} клиентов.");
        }

        static void ShowInventory(AutoServiceService service)
        {
            var inv = service.GetInventory();
            Console.WriteLine("Склад:");
            foreach (var it in inv)
            {
                Console.WriteLine($"{it.Id}) {it.Part.Name} — Количество: {it.Quantity} (Reserved: {it.Reserved})");
            }
        }

        static void ShowPendingPO(AutoServiceService service)
        {
            var pos = service.GetPendingPurchaseOrders();
            Console.WriteLine("Ожидающие поставки:");
            foreach (var p in pos)
            {
                Console.WriteLine($"PO {p.Id} -> Remaining clients: {p.RemainingClientCounter}, Items:");
                foreach (var it in p.Items)
                {
                    Console.WriteLine($"   PartId {it.PartId} x {it.Quantity} (unit {it.UnitPrice:0.00})");
                }
            }
        }

        static void ShowTransactions(AutoServiceDbContext db)
        {
            var txs = db.Transactions.OrderByDescending(t => t.OccurredAt).Take(50).ToList();
            Console.WriteLine("Транзакции (последние):");
            foreach (var t in txs)
            {
                Console.WriteLine($"{t.OccurredAt:yyyy-MM-dd HH:mm} | {t.Type} | {t.Amount:0.00} | {t.Description}");
            }
        }
    }
}