using System;
using RoguelikeGame;

namespace RoguelikeGame
{
    public static class GameRandom
    {
        public static Random rnd = new Random();

        public static bool NextBool()
        {
            return rnd.Next(2) == 0;
        }
    }
}