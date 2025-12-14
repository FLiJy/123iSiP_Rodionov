using System;

namespace RoguelikeGame.Model
{
    class Player
    {
        public string Name;
        public int HP;
        public int MaxHP = 100;
        public Weapon Weapon;
        public Armor Armor;
        public bool Defending;

        Random rnd = new Random();

        public bool IsAlive => HP > 0;

        public Player(string name)
        {
            Name = name;
            HP = MaxHP;
            Weapon = new Weapon("Кулаки", 5);
            Armor = new Armor("Обмотки", 3);
        }

        public int AttackEnemy(Enemy enemy)
        {
            int damage = Weapon.AttackBonus + rnd.Next(5, 15);
            return enemy.TakeDamage(damage);
        }

        public void Defend()
        {
            Defending = true;
        }

        public void TakeDamage(int damage)
        {
            if (Defending)
            {
                int dodge = rnd.Next(100);
                if (dodge < 40)
                {
                    Console.WriteLine("Вы увернулись от атаки!");
                    Defending = false;
                    return;
                }

                int block = rnd.Next(70, 101);
                damage = Math.Max(0, damage - Armor.DefenseBonus);
                damage = damage * (100 - block) / 100;

                Console.WriteLine($"Вы блокировали {block}% урона!");
                Defending = false;
            }

            HP -= damage;
            if (HP < 0) HP = 0;

            Console.WriteLine($"Вы получили {damage} урона!");
        }
    }
}
