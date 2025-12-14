using System;

namespace RoguelikeGame.Model
{
    class Enemy
    {
        public string Name;
        public int HP;
        public int Attack;
        public int Defense;
        public int CritChance;
        public int FreezeChance;
        public bool IgnoreArmor;

        protected static Random rnd = new Random();

        public bool IsAlive => HP > 0;

        public Enemy(string name, int hp, int attack, int defense,
                     int critChance = 0, int freezeChance = 0, bool ignoreArmor = false)
        {
            Name = name;
            HP = hp;
            Attack = attack;
            Defense = defense;
            CritChance = critChance;
            FreezeChance = freezeChance;
            IgnoreArmor = ignoreArmor;
        }

        public virtual bool AttackPlayer(Player player)
        {
            int damage = Attack;
            bool froze = false;

            if (FreezeChance > 0 && rnd.Next(100) < FreezeChance)
                froze = true;

            if (CritChance > 0 && rnd.Next(100) < CritChance)
            {
                damage = (int)(damage * 1.5);
                Console.WriteLine("Критический удар!");
            }

            if (!IgnoreArmor)
                damage -= player.Armor.DefenseBonus;

            if (damage < 0) damage = 0;

            player.TakeDamage(damage);
            return froze;
        }

        public virtual int TakeDamage(int damage)
        {
            damage -= Defense;
            if (damage < 0) damage = 0;

            HP -= damage;
            return damage;
        }
    }

    // === НОВЫЙ МОНСТР ===
    class Slime : Enemy
    {
        public Slime()
            : base("Слизень", 70, 8, 1)
        {
        }

        public override int TakeDamage(int damage)
        {
            damage -= 2;
            if (damage < 0) damage = 0;

            HP -= damage;
            return damage;
        }
    }
}
