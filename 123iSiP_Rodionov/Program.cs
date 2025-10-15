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

   
