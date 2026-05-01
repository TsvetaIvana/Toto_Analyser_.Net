namespace Toto_Analyser
{
    public class Statistics
    {
        private readonly IEnumerable<LotteryDraw> _draws;

        public Statistics(IEnumerable<LotteryDraw> draws)
        {
            _draws = draws ?? Enumerable.Empty<LotteryDraw>();
        }

        public Dictionary<int, int> GetTopFrequentNumbers(int topN = 10)
        {
            var frequencies = _draws
                             .SelectMany(draw => draw.Numbers)
                             .GroupBy(number => number)
                             .ToDictionary(group => group.Key, group => group.Count());

            if (topN == 49)
            {
                for (int i = 1; i <= 49; i++)
                    if (!frequencies.ContainsKey(i))
                        frequencies[i] = 0;
            }

            return frequencies
                .OrderByDescending(kvp => kvp.Value)
                .Take(topN)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public List<(int Number1, int Number2, int Count)> GetHotPairs(int topN = 10)
        {
            return _draws
                .SelectMany(draw =>
                {
                    var pairs = new List<(int, int)>();
                    var sortedNumbers = draw.Numbers.OrderBy(n => n).ToArray();
                    for (int i = 0; i < sortedNumbers.Length - 1; i++)
                    {
                        for (int j = i + 1; j < sortedNumbers.Length; j++)
                        {
                            pairs.Add((sortedNumbers[i], sortedNumbers[j]));
                        }
                    }
                    return pairs;
                })
                .GroupBy(pair => pair)
                .OrderByDescending(group => group.Count())
                .Take(topN)
                .Select(group => (group.Key.Item1, group.Key.Item2, group.Count()))
                .ToList();
        }

        public Dictionary<string, int> GetDecadeDistribution()
        {
            return _draws
                .SelectMany(draw => draw.Numbers)
                .GroupBy(number => GetDecadeLabel(number))
                .OrderBy(group => int.Parse(group.Key.Split('-')[0]))
                .ThenBy(group => group.Key)
                .ToDictionary(group => group.Key, group => group.Count());
        }

        private string GetDecadeLabel(int number) => number switch
        {
            >= 1 and <= 10 => "1-10",
            >= 11 and <= 20 => "11-20",
            >= 21 and <= 30 => "21-30",
            >= 31 and <= 40 => "31-40",
            >= 41 and <= 49 => "41-49",
            _ => "Невалидно число"
        };
    }
}