namespace Toto_Analyser
{
    public class Visualizer
    {
        public void DrawBarChart(Dictionary<int, int> topNumbers)
        {
            if (topNumbers == null || !topNumbers.Any()) return;

            int maxFrequency = topNumbers.Values.Max();
            int maxBarLength = 60;

            foreach (var kvp in topNumbers.OrderByDescending(x => x.Value))
            {
                int currentFreq = kvp.Value;

                int barLength = (int)Math.Round((double)currentFreq / maxFrequency * maxBarLength);

                string bar = new string('#', barLength);

                Console.WriteLine($"{kvp.Key,4} | {bar,-60} {currentFreq}");
            }
        }

        public void DrawHeatMap(Dictionary<int, int> allNumberFrequencies)
        {
            if (allNumberFrequencies == null || allNumberFrequencies.Count == 0) return;

            var sortedFrequencies = allNumberFrequencies.Values.OrderByDescending(f => f).ToList();

            int top30Index = (int)Math.Round(sortedFrequencies.Count * 0.30) - 1;
            int bottom30Index = sortedFrequencies.Count - (int)Math.Round(sortedFrequencies.Count * 0.30);

            int hotThreshold = sortedFrequencies[Math.Max(0, top30Index)];
            int coldThreshold = sortedFrequencies[Math.Min(sortedFrequencies.Count - 1, bottom30Index)];

            for (int i = 1; i <= 49; i++)
            {
                int freq = allNumberFrequencies.ContainsKey(i) ? allNumberFrequencies[i] : 0;

                if (freq >= hotThreshold)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else if (freq <= coldThreshold)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }

                Console.Write($"{i,3} ");

                Console.ResetColor();

                if (i % 7 == 0)
                {
                    Console.WriteLine();
                }
            }
        }
    }
}