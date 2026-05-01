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

        // Зареждаме данните
        var allDraws = await dataLoader.LoadData();
        var drawsList = allDraws.ToList();

        Console.WriteLine($"\nУСПЕХ! Заредени са общо {drawsList.Count} тиража.");

        if (drawsList.Any())
        {
            Console.WriteLine("\n--- Първите 5 записа (трябва да са от 1958 г.) ---");
            foreach (var draw in drawsList.Take(5))
            {
                Console.WriteLine($"Година: {draw.Year} | Тираж №{draw.DrawNumber,2}: {string.Join(", ", draw.Numbers)}");
            }

            Console.WriteLine("\n--- Последните 5 записа (трябва да са от DOCX файловете) ---");
            foreach (var draw in drawsList.TakeLast(5))
            {
                Console.WriteLine($"Година: {draw.Year} | Тираж №{draw.DrawNumber,2}: {string.Join(", ", draw.Numbers)}");
            }
        }

        Console.WriteLine("\nНатиснете клавиш за изход...");
        Console.ReadKey();
    }
}