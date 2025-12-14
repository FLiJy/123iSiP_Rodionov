using System;

namespace RoguelikeGame.Model
{
    class EnemyFactory
    {
        static Random rnd = new Random();

        public static Enemy CreateEnemy()
        {
            int type = rnd.Next(4);

            switch (type)
            {
                case 0:
                    return new Enemy("Гоблин", 50, 10, 3, critChance: 20);

                case 1:
                    return new Enemy("Скелет", 60, 12, 4, ignoreArmor: true);

                case 2:
                    return new Enemy("Маг", 40, 9, 2, freezeChance: 20);

                default:
                    return new Slime();
            }
        }

        public static Enemy CreateBoss()
        {
            int type = rnd.Next(3);

            switch (type)
            {
                case 0:
                    return new Enemy("ВВГ (босс-гоблин)", 100, 15, 6, critChance: 30);

                case 1:
                    return new Enemy("Ковальский (босс-скелет)", 150, 16, 7, ignoreArmor: true);

                default:
                    return new Enemy("Архимаг C++", 90, 18, 4, freezeChance: 30);
            }
        }
    }
}
