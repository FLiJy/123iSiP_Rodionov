using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        List<TextStatistics> statisticsList = new List<TextStatistics>();

        while (true)
        {
            Console.WriteLine("Введите текст длиной минимум 100 символов:");
            string inputText = Console.ReadLine();

            if (inputText.Length >= 100)
            {
                TextStatistics stats = ProcessText(inputText);

                statisticsList.Add(stats);

                DisplayStats(stats);   

                Console.WriteLine("\nХотите обработать ещё один текст? (Да/Нет)");
                string answer = Console.ReadLine().ToLower();
                if (answer != "да")
                    break;
            }
            else
            {
                Console.WriteLine("Ошибка: введённый текст меньше 100 символов.");
            }
        }

        if (statisticsList.Count > 1)
        {
            foreach (var stat in statisticsList)
            {
                Console.WriteLine("\nСтатистика предыдущего текста:");
                DisplayStats(stat);
            }
        }
    }
        private static TextStatistics ProcessText(string text)
        {
            var words = SplitIntoWords(text);                  // Разбор на слова
            int wordCount = CountWords(words);                 // Подсчёт количества слов
            string shortestWord = FindShortestWord(words);     // Поиск короткого слова
            int sentenceCount = CountSentences(text);          // Счёт предложений
            int vowelsCount = CountVowels(text);               // Количество гласных
            int consonantsCount = CountConsonants(text);       // Количество согласных
            string longestWord = FindLongestWord(words);       // Самое длинное слово
            Dictionary<char, int> letterFrequency = CalculateLetterFrequency(text); // Частота букв

            return new TextStatistics(wordCount, shortestWord, sentenceCount,
                                      vowelsCount, consonantsCount, longestWord, letterFrequency);
            }
        private static string[] SplitIntoWords(string text)
        {
            char[] separators = { ' ', '\t', '\n' };
            return text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }

        // Подсчёт общего кол-ва слов
        private static int CountWords(string[] words)
        {
            return words.Length;
        }

        // Поиск самого короткого слова среди списка слов
        private static string FindShortestWord(string[] words)
        {
            string shortest = words[0];
            for (int i = 1; i < words.Length; i++)
            {
                if (words[i].Length < shortest.Length)
                    shortest = words[i];
            }
            return shortest;
        }
        // Подсчёт количества предложений в тексте
        private static int CountSentences(string text)
        {
            char[] endMarks = { '.', '!', '?' };              // Знаки конца предложения
            int count = 0;
            foreach (char ch in text)
            {
                if (Array.IndexOf(endMarks, ch) >= 0)
                    count++;
            }
            return count;
        }

        // Подсчёт кол-ва гласных букв
        private static int CountVowels(string text)
        {
            string vowels = "aeiouyAEIOUY";                   // Гласные буквы англ алфавита
            int count = 0;
            foreach (char ch in text)
            {
                if (vowels.Contains(ch))
                    count++;
            }
            return count;
        }

        // Подсчёт кол-ва согласных букв
        private static int CountConsonants(string text)
        {
            string consonants = "bcdfghjklmnpqrstvwxyzBCDFGHJKLMNPQRSTVWXYZ";
            int count = 0;
            foreach (char ch in text)
            {
                if (consonants.Contains(ch))
                    count++;
            }
            return count;
        }

        // Поиск самого длинного слова
        private static string FindLongestWord(string[] words)
        {
            string longest = words[0];
            for (int i = 1; i < words.Length; i++)
            {
                if (words[i].Length > longest.Length)
                    longest = words[i];
            }
            return longest;
        }

        // Статистика частотности букв
        private static Dictionary<char, int> CalculateLetterFrequency(string text)
        {
            Dictionary<char, int> frequencyDict = new Dictionary<char, int>();
            foreach (char ch in text.ToCharArray())
            {
                if (!frequencyDict.ContainsKey(ch))
                    frequencyDict[ch] = 0;
                frequencyDict[ch]++;
            }
            return frequencyDict;
        }
        // Отображение статистики 
        private static void DisplayStats(TextStatistics stats)
        {
            Console.WriteLine($"Количество слов: {stats.WordCount}");
            Console.WriteLine($"Самое короткое слово: '{stats.ShortestWord}'");
            Console.WriteLine($"Количество предложений: {stats.SentenceCount}");
            Console.WriteLine($"Количество гласных букв: {stats.VowelsCount}");
            Console.WriteLine($"Количество согласных букв: {stats.ConsonantsCount}");
            Console.WriteLine($"Самое длинное слово: '{stats.LongestWord}'");
            Console.WriteLine("Частота встречаемости каждой буквы:");
            foreach (var pair in stats.LetterFrequency)
            {
                Console.WriteLine($"{pair.Key}: {pair.Value}");
            }
        }
      }
    // Класс для хранения статистической информации по каждому тексту
    public class TextStatistics
    {
        public readonly int WordCount;
        public readonly string ShortestWord;
        public readonly int SentenceCount;
        public readonly int VowelsCount;
        public readonly int ConsonantsCount;
        public readonly string LongestWord;
        public readonly Dictionary<char, int> LetterFrequency;

        public TextStatistics(int wordCount, string shortestWord, int sentenceCount,
                              int vowelsCount, int consonantsCount, string longestWord,
                              Dictionary<char, int> letterFrequency)
        {
            this.WordCount = wordCount;
            this.ShortestWord = shortestWord;
            this.SentenceCount = sentenceCount;
            this.VowelsCount = vowelsCount;
            this.ConsonantsCount = consonantsCount;
            this.LongestWord = longestWord;
            this.LetterFrequency = letterFrequency;
        }
    }

