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
