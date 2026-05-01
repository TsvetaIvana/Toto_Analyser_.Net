using Toto_Analyser;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("==============================================");
        Console.WriteLine("       ЗАРЕЖДАНЕ НА БАЗАТА ДАННИ...         ");
        Console.WriteLine("==============================================\n");

        using DataLoader dataLoader = new DataLoader();
        var allDraws = (await dataLoader.LoadData()).ToList();

        if (!allDraws.Any())
        {
            Console.WriteLine("Грешка: Не бяха заредени никакви данни. Програмата приключва.");
            return;
        }

        List<LotteryDraw> filteredDraws = new List<LotteryDraw>();
        bool isPeriodSelected = false;
        var visualizer = new Visualizer();

        while (true)
        {
            Console.Clear();
            Console.Write("\x1b[3J"); // Изчистване на конзолния буфер за скрол, за да се предотврати показването на стари данни при превъртане нагоре
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("""
                    ==============================================
                                  ТОТО АНАЛИЗАТОР               
                    ==============================================
                       [1]  Избери период (от година - до година)
                       [2]  Топ N най-чести числа
                       [3]  Горещи двойки
                       [4]  Разпределение по десетици

                       [0]  Изход
                    ==============================================
                    """);
            Console.Write("   Избор: ");
            Console.ResetColor();

            string input = Console.ReadLine();
            Console.WriteLine();

            switch (input)
            {
                case "0":
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\nБлагодарим ви, че използвахте Тото Анализатор. Довиждане!");
                    Console.ResetColor();
                    return;

                case "1":
                    FilterData(allDraws, out filteredDraws, out isPeriodSelected);
                    break;

                case "2":
                case "3":
                case "4":
                    if (!isPeriodSelected)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("\nМоля, първо изберете период (Опция 1), преди да правите анализи!");
                        Console.ResetColor();
                    }
                    else
                    {
                        var stats = new Statistics(filteredDraws);

                        switch (input)
                        {
                            case "2":
                                GetTopNumbersAndHeatmap(visualizer, stats);
                                break;

                            case "3":
                                GetTopHotPairs(stats);
                                break;

                            case "4":
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("\n--- Разпределение на всички изтеглени числа в групи по 10 ---");
                                Console.ResetColor();
                                Console.WriteLine();
                                var decades = stats.GetDecadeDistribution();
                                foreach (var kvp in decades)
                                {
                                    Console.WriteLine($"Диапазон {kvp.Key,-5} -> {kvp.Value,5} изтеглени числа");
                                }
                                break;
                        }
                    }
                    break;

                default:
                    Console.WriteLine("\nНевалиден избор. Моля, въведете число от менюто.");
                    break;
            }

            Console.WriteLine("\nНатиснете клавиш за продължаване...");
            Console.ReadKey();
        }
    }

    private static void GetTopHotPairs(Statistics stats)
    {
        Console.Write("Въведете колко двойки да се покажат (N): ");
        if (!int.TryParse(Console.ReadLine(), out int pairsCount) || pairsCount <= 0) pairsCount = 10;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n--- Показване на топ {pairsCount} най-често срещани двойки числа ---");
        Console.ResetColor();
        Console.WriteLine();
        var hotPairs = stats.GetHotPairs(pairsCount);
        int rankPair = 1;
        foreach (var pair in hotPairs)
        {
            Console.WriteLine($"{rankPair,2}. Двойката ({pair.Number1,2}, {pair.Number2,2}) се е паднала {pair.Count,4} пъти");
            rankPair++;
        }
    }

    private static void GetTopNumbersAndHeatmap(Visualizer visualizer, Statistics stats)
    {
        Console.Write("Въведете колко числа да се покажат (N): ");
        if (!int.TryParse(Console.ReadLine(), out int n) || n <= 0) n = 10;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n--- Показване на топ {n} числа и обща топлинна карта за периода ---");
        Console.ResetColor();
        Console.WriteLine();
        visualizer.DrawBarChart(stats.GetTopFrequentNumbers(n));
        Console.WriteLine();
        visualizer.DrawHeatMap(stats.GetTopFrequentNumbers(49));
    }

    private static void FilterData(List<LotteryDraw> allDraws, out List<LotteryDraw> filteredDraws, out bool isPeriodSelected)
    {
        Console.Write("Въведете начална година (напр. 1958): ");
        if (!int.TryParse(Console.ReadLine(), out int startYear)) startYear = 1958;

        Console.Write("Въведете крайна година (напр. 2026): ");
        if (!int.TryParse(Console.ReadLine(), out int endYear)) endYear = DateTime.Now.Year;

        filteredDraws = allDraws.Where(d => d.Year >= startYear && d.Year <= endYear).ToList();

        if (filteredDraws.Any())
        {
            isPeriodSelected = true;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\nУспешно избрахте период {startYear}-{endYear}. Намерени са {filteredDraws.Count} тиража.");
            Console.ResetColor();
        }
        else
        {
            isPeriodSelected = false;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\nВнимание: Няма намерени тиражи за периода {startYear}-{endYear}!");
            Console.ResetColor();
        }
        return;
    }
}