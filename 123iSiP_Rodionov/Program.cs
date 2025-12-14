using System;
using RoguelikeGame.Model;

namespace RoguelikeGame
{
    class Program
    {
        static Random rnd = new Random();

        static void Main(string[] args)
        {
            Player player = new Player("Игрок");
            int turn = 1;

            Console.WriteLine("Добро пожаловать в мини-рогалик!\n");

            while (player.IsAlive)
            {
                Console.WriteLine($"\n----- Ход {turn} -----");

                if (turn % 10 == 0)
                {
                    Enemy boss = EnemyFactory.CreateBoss();
                    Console.WriteLine($"Появился босс: {boss.Name}!");
                    Battle(player, boss);
                }
                else
                {
                    int eventType = rnd.Next(2);

                    if (eventType == 0)
                    {
                        Enemy enemy = EnemyFactory.CreateEnemy();
                        Console.WriteLine($"Вы встретили врага: {enemy.Name}!");
                        Battle(player, enemy);
                    }
                    else
                    {
                        Console.WriteLine("Вы нашли сундук!");
                        ChestEvent(player);
                    }
                }

                if (!player.IsAlive)
                {
                    Console.WriteLine("Вы погибли. Игра окончена.");
                    break;
                }

                turn++;
            }
        }

        static void Battle(Player player, Enemy enemy)
        {
            bool playerFrozen = false;

            while (player.IsAlive && enemy.IsAlive)
            {
                Console.WriteLine(
                    $"\n{player.Name}: {player.HP} HP | {enemy.Name}: {enemy.HP} HP");

                if (!playerFrozen)
                {
                    Console.Write("Ваш ход (1 - Атака, 2 - Защита): ");
                    string choice = Console.ReadLine();

                    if (choice == "1")
                    {
                        int damage = player.AttackEnemy(enemy);
                        Console.WriteLine($"Вы нанесли {damage} урона {enemy.Name}!");
                    }
                    else
                    {
                        player.Defend();
                        Console.WriteLine("Вы встали в защитную стойку!");
                    }
                }
                else
                {
                    Console.WriteLine("Вы заморожены и пропускаете ход!");
                    playerFrozen = false;
                }

                if (!enemy.IsAlive)
                    break;

                bool froze = enemy.AttackPlayer(player);
                if (froze)
                {
                    Console.WriteLine(
                        $"{enemy.Name} наложил заморозку! Вы пропустите следующий ход!");
                    playerFrozen = true;
                }
            }

            Console.WriteLine($"{enemy.Name} повержен!");
        }

        static void ChestEvent(Player player)
        {
            int drop = rnd.Next(3);

            if (drop == 0)
            {
                Console.WriteLine(
                    "Вы нашли лечебное зелье! HP полностью восстановлено!");
                player.HP = player.MaxHP;
            }
            else if (drop == 1)
            {
                Weapon newWeapon = Weapon.GenerateRandomWeapon();

                Console.WriteLine(
                    $"Вы нашли оружие: {newWeapon.Name} (+{newWeapon.AttackBonus} атаки)");
                Console.WriteLine(
                    $"Текущее оружие: {player.Weapon.Name} (+{player.Weapon.AttackBonus})");

                Console.Write("Взять новое? (y/n): ");
                if (Console.ReadLine()?.ToLower() == "y")
                    player.Weapon = newWeapon;
            }
            else
            {
                Armor newArmor = Armor.GenerateRandomArmor();

                Console.WriteLine(
                    $"Вы нашли доспех: {newArmor.Name} (+{newArmor.DefenseBonus} защиты)");
                Console.WriteLine(
                    $"Текущие доспехи: {player.Armor.Name} (+{player.Armor.DefenseBonus})");

                Console.Write("Взять новые? (y/n): ");
                if (Console.ReadLine()?.ToLower() == "y")
                    player.Armor = newArmor;
            }
        }
    }
}
