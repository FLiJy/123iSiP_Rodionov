using System;

namespace RoguelikeGame 
{
    class Program //класс программа
    {
        static Random rnd = new Random(); //генератор случ чисел(рандомайзер)

        static void Main(string[] args) //вход в консоль(программа)
        {
            Player player = new Player("Игрок"); //создаем игрока
            int turn = 1; //номер текущего хода

            Console.WriteLine("Добро пожаловать в мини-рогалик!\n");

            while (player.IsAlive) //цикл идет пока игрок жив
            {
                Console.WriteLine($"\n----- Ход {turn} -----");
                if (turn % 10 == 0) //каждые 10 ходов появляется босс
                {
                    Enemy boss = Enemy.GenerateBoss(); //генерируем случ босса
                    Console.WriteLine($"Появился босс: {boss.Name}!"); //сообщаем игроку
                    Battle(player, boss); //и запускаем битву с ним
                }
                else //обычный ход с врагом или сундуком
                {
                    int eventType = rnd.Next(2); // 0-враг, 1-сундук
                    if (eventType == 0)
                    {
                        Enemy enemy = Enemy.GenerateEnemy(); //тоже самое что и с боссом
                        Console.WriteLine($"Вы встретили врага: {enemy.Name}!");
                        Battle(player, enemy);
                    }
                    else
                    {
                        Console.WriteLine("Вы нашли сундук!"); 
                        ChestEvent(player);//обработка предмета из сундука
                    }
                }

                if (!player.IsAlive) //погиб... Увы
                {
                    Console.WriteLine("Вы погибли. Игра окончена.");
                    break;
                }

                turn++; //след ход
            }
        }
        static void Battle(Player player, Enemy enemy) //метод боя с врагом
        {
            bool playerFrozen = false; //пропуск хода 

            while (player.IsAlive && enemy.IsAlive) //Цикл боя до смерти игрока или врага
            {
                Console.WriteLine($"\n{player.Name}: {player.HP} HP | {enemy.Name}: {enemy.HP} HP");//вывод информации

                if (!playerFrozen) // если игрко не заморожен, он совершает действие
                {
                    Console.Write("Ваш ход (1 - Атака, 2 - Защита): ");
                    string choice = Console.ReadLine();

                    if (choice == "1") //атака
                    {
                        int damage = player.AttackEnemy(enemy);
                        Console.WriteLine($"Вы нанесли {damage} урона {enemy.Name}!");
                    }
                    else
                    {
                        player.Defend(); //защита
                        Console.WriteLine("Вы встали в защитную стойку!");
                    }
                }
                else //пропуск хода из-за заморохки
                {
                    Console.WriteLine("Вы заморожены и пропускаете ход!");
                    playerFrozen = false; //снимаем эффект, он действует 1 ход
                }

                if (!enemy.IsAlive) break; //враг мертв, выходим из боя

                //ход врага
                bool froze = enemy.AttackPlayer(player);//враг атакаует, при заморозке; Метод возвращает тру, если заморожен
                if (froze)
                {
                    Console.WriteLine($"{enemy.Name} наложил заморозку! Вы пропустите следующий ход!");
                    playerFrozen = true;
                }
            }

            if (enemy.HP <= 0)//враг-всё
                Console.WriteLine($"{enemy.Name} повержен!");
        }

        static void ChestEvent(Player player) //обработка события с сундуком
        {
            int drop = rnd.Next(3); //3 типа дропа(0, 1 ,2)
            if (drop == 0) // если выпало зелье
            {
                Console.WriteLine("Вы нашли лечебное зелье! HP полностью восстановлено!");
                player.HP = player.MaxHP;
            }
            else if (drop == 1) // если выпало оружие
            {
                Weapon newWeapon = Weapon.GenerateRandomWeapon();
                Console.WriteLine($"Вы нашли оружие: {newWeapon.Name} (+{newWeapon.AttackBonus} атаки)");
                Console.WriteLine($"Текущее оружие: {player.Weapon.Name} (+{player.Weapon.AttackBonus})");
                Console.Write("Взять новое? (y/n): ");
                if (Console.ReadLine()?.ToLower() == "y") //если игрок согл
                    player.Weapon = newWeapon; //меняем 
            }
            else // если выпали доспехи
            {
                Armor newArmor = Armor.GenerateRandomArmor();
                Console.WriteLine($"Вы нашли доспех: {newArmor.Name} (+{newArmor.DefenseBonus} защиты)");
                Console.WriteLine($"Текущие доспехи: {player.Armor.Name} (+{player.Armor.DefenseBonus})");
                Console.Write("Взять новые? (y/n): ");
                if (Console.ReadLine()?.ToLower() == "y") //если игрок согл
                    player.Armor = newArmor; //меняем
            }
        }
    }

    class Player //класс игрока, хранит хп, эквип, и логику действий
    {
        public string Name;
        public int HP;
        public int MaxHP = 100;
        public Weapon Weapon;
        public Armor Armor;
        public bool Defending = false;
        Random rnd = new Random(); //локальный для игрока генератор чисел

        public bool IsAlive => HP > 0; // жив ли игрок

        public Player(string name) //конструктор игрока(что идет в базе)
        {
            Name = name;
            HP = MaxHP;
            Weapon = new Weapon("Кулаки", 5);
            Armor = new Armor("Обмотки", 3);
        }

        public int AttackEnemy(Enemy enemy) //метод атаки по врагу
        {
            int damage = Weapon.AttackBonus + rnd.Next(5, 15); //Урон = Бонус от оружия +случ часть
            damage -= enemy.Defense; //усменьшаем урон на броню врага
            if (damage < 0) damage = 0; //убеждаемся что урон не 0
            enemy.HP -= damage; // применяем урок
            return damage; //возвращаем урон(для вывода) нанесенный
        }

        public void Defend() // для защиты метод
        {
            Defending = true; //используется при получении урона
        }

        public void TakeDamage(int damage) //метод обработки получения урона
        {
            if (Defending) //если защищаемся
            {
                int dodge = rnd.Next(100); //случ число от 0 до 99
                if (dodge < 40) // 40 проц на додж
                {
                    Console.WriteLine("Вы увернулись от атаки!");
                    Defending = false; // режим защиты снимается
                    return;
                }

                int blockPercent = rnd.Next(70, 101); //случ блок от 70 до 100

                int reducedDamage = Math.Max(0, damage - Armor.DefenseBonus);//уменьшаем урон на знач брони

                int finalDamage = reducedDamage * (100 - blockPercent) / 100; //урон после блока

                Console.WriteLine($"Вы блокировали {blockPercent}% урона!"); //информируем игрока сколько блокировано
                damage = finalDamage;
                Defending = false;
            }

            HP -= damage; //применяем дамаг к здоровью игрока
            if (HP < 0) HP = 0; // не допускаем отриц значения
            Console.WriteLine($"Вы получили {damage} урона!"); //вывод
        }
    }
    class Enemy // класс для врага(хар-ки и лгика атаки)
    {
        public string Name; //информация? для врагов(если так можно назвать)
        public int HP;
        public int Attack;
        public int Defense;
        public double CritChance;
        public double FreezeChance;
        public bool IgnoreArmor;

        static Random rnd = new Random(); //общитй генератор для enemy

        public bool IsAlive => HP > 0; //свойство, показывающая жив ли враг

        public bool AttackPlayer(Player player) //метод атаки по врагу
        {
            int damage = Attack; //урон 
            bool froze = false; //заморозка

            if (FreezeChance > 0 && rnd.Next(100) < FreezeChance) //шанс замороозки
                froze = true; //выпало = заморозка прошла

            if (CritChance > 0 && rnd.Next(100) < CritChance) //шанс крита
            {
                damage = (int)(damage * 1.5); //крит урон умнодает урон на 1.5
                Console.WriteLine("Критический удар!");
            }

            if (!IgnoreArmor) //игнор брони
                damage -= player.Armor.DefenseBonus;

            if (damage < 0) damage = 0;
            player.TakeDamage(damage);//применяем урон к игроку
            return froze; //возвр был ли эффект заморозки 
        }

        public static Enemy GenerateEnemy()//метод для обычного врага
        {
            int type = rnd.Next(3); // 0, 1 ,2 выбирается случайно
            switch (type) //в зависимости от числа выбираем конкретный тип врага
            {
                case 0: return new Enemy { Name = "Гоблин", HP = 50, Attack = 10, Defense = 3, CritChance = 20 };
                case 1: return new Enemy { Name = "Скелет", HP = 60, Attack = 12, Defense = 4, IgnoreArmor = true };
                default: return new Enemy { Name = "Маг", HP = 40, Attack = 9, Defense = 2, FreezeChance = 20 };
            }
        }

        public static Enemy GenerateBoss()//метод для боссов
        {
            int bossType = rnd.Next(4); // 0, 1 , 2 ,3
            switch (bossType) //тоже самое
            {
                case 0:
                    return new Enemy { Name = "ВВГ (босс-гоблин)", HP = (int)(50 * 2.0), Attack = (int)(10 * 1.5), Defense = (int)(3 * 1.2), CritChance = 30 };
                case 1:
                    return new Enemy { Name = "Ковальский (босс-скелет)", HP = (int)(60 * 2.5), Attack = (int)(12 * 1.3), Defense = (int)(4 * 1.4), IgnoreArmor = true };
                case 2:
                    return new Enemy { Name = "Архимаг C++", HP = (int)(40 * 1.8), Attack = (int)(9 * 1.6), Defense = (int)(2 * 1.1), FreezeChance = 30 };
                default:
                    return new Enemy { Name = "Пестов С--", HP = (int)(60 * 1.3), Attack = (int)(12 * 1.8), Defense = (int)(4 * 0.6), IgnoreArmor = true, FreezeChance = 35 };
            }
        }
    }

    class Weapon //класс оружие
    {
        public string Name; //название
        public int AttackBonus; //атак бонус от оружия

        public Weapon(string name, int attack) //конструктор оружия
        {
            Name = name; //устанвалием имя
            AttackBonus = attack; //устанавливаем бонус к атаке
        }

        static string[] names = { "Меч", "Моргенштерн", "Алебарда", "Копьё крестьянина" }; //имена
        public static Weapon GenerateRandomWeapon() //генератор случайного значения для класса
        {
            string name = names[new Random().Next(names.Length)]; //имя
            int bonus = new Random().Next(5, 20); //случ бонус к атаке от 5 до 19
            return new Weapon(name, bonus); //обновляем значения
        }
    }

    class Armor //класс доспехов
    {
        public string Name; //название
        public int DefenseBonus; // бонус к защите

        public Armor(string name, int def) //конструктор доспехов
        {
            Name = name; //устанавливаем название
            DefenseBonus = def; //устанвливаем бонус к защите
        }

        static string[] names = { "Тряпки торговца Абиля", "Броня школы петуха", "Кольчюга", "Стальной доспех" };
        public static Armor GenerateRandomArmor() //генератор случайного доспеха 
        {
            string name = names[new Random().Next(names.Length)]; //рандомное имя
            int bonus = new Random().Next(3, 15);// рандомный бонус к защите
            return new Armor(name, bonus); //обновляем значение
        }
    }
}


