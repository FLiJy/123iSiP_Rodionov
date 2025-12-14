using System;

namespace RoguelikeGame.Model
{
    class Armor
    {
        public string Name;
        public int DefenseBonus;

        static string[] names =
        {
            "Тряпки торговца Абиля",
            "Броня школы петуха",
            "Кольчуга",
            "Стальной доспех"
        };

        public Armor(string name, int def)
        {
            Name = name;
            DefenseBonus = def;
        }

        public static Armor GenerateRandomArmor()
        {
            Random rnd = new Random();
            string name = names[rnd.Next(names.Length)];
            int bonus = rnd.Next(3, 15);

            return new Armor(name, bonus);
        }
    }
}
