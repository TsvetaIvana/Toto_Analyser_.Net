using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Toto_Analyser
{
    public class DataLoader : IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl = "https://info.toto.bg";
        private readonly string cacheFolder = "TotoData";

        public DataLoader()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
            httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "bg-BG,bg;q=0.9,en-US;q=0.8,en;q=0.7");

            if (!Directory.Exists(cacheFolder)) Directory.CreateDirectory(cacheFolder);
        }

        public async Task<IEnumerable<LotteryDraw>> LoadData()
        {
            var allDraws = new List<LotteryDraw>();

            var fileLinks = new Dictionary<string, int>(); // Ключ: URL или локален път, Стойност: година

            try
            {
                await ExtractFileLinks(fileLinks);
            }
            catch { Console.WriteLine("Грешка при изтеглянето на нови файлове от сайта"); } 

            if (fileLinks.Count == 0)
            {
                await HandleLocalFiles(allDraws);
                return allDraws;
            }

            await HandleNewFiles(allDraws, fileLinks);
            return allDraws.OrderBy(d => d.Year).ThenBy(d => d.DrawNumber).ToList();
        }

        private async Task HandleNewFiles(List<LotteryDraw> allDraws, Dictionary<string, int> fileLinks)
        {
            Console.WriteLine($"Намерени са {fileLinks.Count} файла в сайта за обработка.\n");
            foreach (var kvp in fileLinks.OrderBy(x => x.Value)) // Сортираме по година
            {
                string fullUrl = kvp.Key.StartsWith("/") ? $"{baseUrl}{kvp.Key}" : kvp.Key;
                int year = kvp.Value;

                string extension = fullUrl.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) ? ".docx" : ".txt";
                string localFilePath = Path.Combine(cacheFolder, $"{year}{extension}");

                if (extension == ".txt") allDraws.AddRange(await ParseTxtFile(fullUrl, year, localFilePath));
                else if (extension == ".docx") allDraws.AddRange(await ParseDocxFile(fullUrl, year, localFilePath));
            }
        }

        private async Task HandleLocalFiles(List<LotteryDraw> allDraws)
        {
            Console.WriteLine("Сайтът временно не е достъпен. Зареждане на локалния архив...");
            if (Directory.Exists(cacheFolder))
            {
                var localFiles = Directory.GetFiles(cacheFolder).OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f))).ToArray();
                Console.WriteLine($"Намерени са {localFiles.Length} локални файла за обработка.\n");

                foreach (var localFilePath in localFiles)
                {
                    if (int.TryParse(Path.GetFileNameWithoutExtension(localFilePath), out int year))
                    {
                        if (localFilePath.EndsWith(".txt")) allDraws.AddRange(await ParseTxtFile("", year, localFilePath));
                        else if (localFilePath.EndsWith(".docx")) allDraws.AddRange(await ParseDocxFile("", year, localFilePath));
                    }
                }
            }
        }

        private async Task ExtractFileLinks(Dictionary<string, int> fileLinks)
        {
            Console.WriteLine("Проверка за нови данни от сайта...");
            var mainPageResponse = await httpClient.GetStringAsync($"{baseUrl}/statistika/6x49");

            // Хваща едновременно линка и годината от самия HTML бутон (напр. >2017<)
            string pattern = @"href\s*=\s*[""'](/content/files/[^""']+\.(?:txt|docx))[""'][^>]*>\s*(\d{4})\s*<";
            MatchCollection matches = Regex.Matches(mainPageResponse, pattern, RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                string url = match.Groups[1].Value;
                int year = int.Parse(match.Groups[2].Value);
                fileLinks[url] = year;
            }
        }

        private async Task<List<LotteryDraw>> ParseTxtFile(string fileUrl, int year, string localPath)
        {
            var draws = new List<LotteryDraw>();
            try
            {
                string content = await GetDataTxt(fileUrl, localPath);

                var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var formatNew = Regex.Match(line, @"(\d+)/\d{4},\s*\D+\d+:\s*([\d\s]+)");

                    if (formatNew.Success)
                    {
                        HandleFormatNew(year, draws, formatNew);
                        continue;
                    }

                    HandleOldFormatsTxt(year, draws, line);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" -> Грешка при TXT файл ({year}): {ex.Message}");
            }
            return draws;
        }

        private static void HandleOldFormatsTxt(int year, List<LotteryDraw> draws, string line)
        {
            //Разделяне по всички възможни символи, които могат да се появят между числата
            var parts = line.Split(new char[] { ' ', '\t', ',', ';', '-', '?', '–', '—' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 7)
            {
                // Взимане на първото парче и махане на всички нецифрови символи от него
                string cleanDrawNum = new string(parts[0].Where(char.IsDigit).ToArray());

                if (!string.IsNullOrEmpty(cleanDrawNum) && int.TryParse(cleanDrawNum, out int drawNum))
                {
                    // Взимане на останалите парчета, които съдържат само цифри
                    var rawNumbers = parts.Skip(1)
                        .Select(p => new string(p.Where(char.IsDigit).ToArray()))
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();

                    // Обработване на порции от по 6
                    for (int i = 0; i <= rawNumbers.Count - 6; i += 6)
                    {
                        try
                        {
                            var numbers = rawNumbers.Skip(i).Take(6).Select(int.Parse).ToArray();
                            // Проверка дали това са 6 валидни тото числа
                            if (numbers.All(n => n >= 1 && n <= 49))
                            {
                                draws.Add(new LotteryDraw { DrawNumber = drawNum, Numbers = numbers, Year = year });
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        private async Task<string> GetDataTxt(string fileUrl, string localPath)
        {
            string content;
            if (File.Exists(localPath))
                content = await File.ReadAllTextAsync(localPath, System.Text.Encoding.UTF8);
            else
            {
                var bytes = await httpClient.GetByteArrayAsync(fileUrl);
                content = System.Text.Encoding.UTF8.GetString(bytes);
                await File.WriteAllTextAsync(localPath, content, System.Text.Encoding.UTF8);
                await Task.Delay(500);
            }

            return content;
        }

        private static void HandleFormatNew(int year, List<LotteryDraw> draws, Match formatА)
        {
            if (int.TryParse(formatА.Groups[1].Value, out int drawNum))
            {
                var numbers = formatА.Groups[2].Value
                    .Trim()
                    .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToArray();

                if (numbers.Length == 6 && numbers.All(n => n >= 1 && n <= 49))
                    draws.Add(new LotteryDraw { DrawNumber = drawNum, Numbers = numbers, Year = year });
            }
        }

        private async Task<List<LotteryDraw>> ParseDocxFile(string fileUrl, int year, string localPath)
        {
            var draws = new List<LotteryDraw>();
            try
            {
                //Изтегляне или четене от кеша
                if (!File.Exists(localPath))
                {
                    var fileBytes = await httpClient.GetByteArrayAsync(fileUrl);
                    await File.WriteAllBytesAsync(localPath, fileBytes);
                    await Task.Delay(500);
                }

                using Stream stream = File.OpenRead(localPath);
                using WordprocessingDocument wordDoc = WordprocessingDocument.Open(stream, false);
                var body = wordDoc.MainDocumentPart.Document.Body;

                var lines = new List<string>();

                ExtractDataFromDocx(body, lines);

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var formatNew = Regex.Match(line, @"(\d+)/\d{4},\s*\D+\d+:\s*([\d\s]+)");

                    if (formatNew.Success)
                    {
                        HandleFormatNew(year, draws, formatNew);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" -> Грешка при DOCX файл ({year}): {ex.Message}");
            }

            return draws;
        }

        private static void ExtractDataFromDocx(Body body, List<string> lines)
        {
            foreach (var element in body.ChildElements)
            {
                if (element is Paragraph p)
                {
                    var texts = p.Descendants<Text>().Select(t => t.Text);
                    lines.Add(string.Join(" ", texts));
                }
                else if (element is Table t)
                {
                    foreach (var row in t.Descendants<TableRow>())
                    {
                        var cells = row.Descendants<TableCell>().Select(c => c.InnerText.Trim());
                        lines.Add(string.Join(" ", cells));
                    }
                }
            }
        }

        public void Dispose() => httpClient?.Dispose();
    }
}