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

        
