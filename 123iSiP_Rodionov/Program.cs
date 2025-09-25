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

                // Добавляем статистику текущего текста в общий список
                statisticsList.Add(stats);

                DisplayStats(stats);   // Показываем текущую статистику

                // Спрашиваем пользователя, хочет ли он ввести новый текст
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
        // Если были обработаны хотя бы два текста, выводим статистику всех предыдущих
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
            var words = SplitIntoWords(text);                  // Разбиваем текст на слова
            int wordCount = CountWords(words);                 // Подсчёт числа слов
            string shortestWord = FindShortestWord(words);     // Поиск самого короткого слова
            int sentenceCount = CountSentences(text);          // Подсчёт предложений
            int vowelsCount = CountVowels(text);               // Количество гласных
            int consonantsCount = CountConsonants(text);       // Количество согласных
            string longestWord = FindLongestWord(words);       // Самое длинное слово
            Dictionary<char, int> letterFrequency = CalculateLetterFrequency(text); // Частота букв

            return new TextStatistics(wordCount, shortestWord, sentenceCount,
                                      vowelsCount, consonantsCount, longestWord, letterFrequency);
            }
        // Функция для разделения строки на отдельные слова
        private static string[] SplitIntoWords(string text)
        {
            char[] separators = { ' ', '\t', '\n' };
            return text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }

        // Подсчёт общего количества слов
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

        // Подсчёт количества гласных букв
        private static int CountVowels(string text)
        {
            string vowels = "aeiouyAEIOUY";                   // Гласные буквы латинского алфавита
            int count = 0;
            foreach (char ch in text)
            {
                if (vowels.Contains(ch))
                    count++;
            }
            return count;
        }

        // Подсчёт количества согласных букв
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


       