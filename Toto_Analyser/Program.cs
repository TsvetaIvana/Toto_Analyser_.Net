using System;
using System.Linq;
using System.Threading.Tasks;
using Toto_Analyser;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("============================================");
        Console.WriteLine("           ТОТО АНАЛИЗАТОР - ТЕСТ           ");
        Console.WriteLine("============================================\n");

        using DataLoader dataLoader = new DataLoader();

        var allDraws = await dataLoader.LoadData();
        var drawsList = allDraws.ToList();

        Console.WriteLine($"Заредени са общо {drawsList.Count} тиража.");

        if (drawsList.Any())
        {
            var stats = new Statistics(drawsList);

            // --- ТЕСТ 1: Топ 10 числа ---
            Console.WriteLine("\n--> ТОП 10 НАЙ-ЧЕСТО ИЗТЕГЛЯНИ ЧИСЛА:");
            var topNumbers = stats.GetTopFrequentNumbers(10);
            int rankNum = 1;
            foreach (var kvp in topNumbers)
            {
                // Забележете {kvp.Key,2} за числата и {kvp.Value,4} за пътите
                Console.WriteLine($"{rankNum,2}. Число {kvp.Key,2} се е паднало {kvp.Value,4} пъти");
                rankNum++;
            }

            // --- ТЕСТ 2: Топ 10 Горещи двойки ---
            Console.WriteLine("\n--> ТОП 10 ГОРЕЩИ ДВОЙКИ:");
            var hotPairs = stats.GetHotPairs(10);
            int rankPair = 1;
            foreach (var pair in hotPairs)
            {
                Console.WriteLine($"{rankPair,2}. Двойката ({pair.Number1,2}, {pair.Number2,2}) се е паднала {pair.Count,4} пъти");
                rankPair++;
            }

            // --- ТЕСТ 3: Разпределение по десетици ---
            Console.WriteLine("\n--> РАЗПРЕДЕЛЕНИЕ ПО ДЕСЕТИЦИ:");
            var decades = stats.GetDecadeDistribution();
            foreach (var kvp in decades)
            {
                // Тук ползваме минус (-5), за да подравним текста наляво
                Console.WriteLine($"Диапазон {kvp.Key,-5} -> {kvp.Value,5} изтеглени числа");
            }
        }

        Console.WriteLine("\n============================================");
        Console.WriteLine("Натиснете клавиш за изход...");
        Console.ReadKey();
    }
}