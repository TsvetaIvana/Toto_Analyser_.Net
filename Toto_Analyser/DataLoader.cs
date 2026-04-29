using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toto_Analyser
{
    public class DataLoader : IDisposable
    {
        public readonly HttpClient httpClient;
        public DataLoader()
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://info.toto.bg/content/files/stats-tiraji/");
        }

        public async Task<Dictionary<int, DrawsYear>> LoadData()
        {
            var data = new Dictionary<int, DrawsYear>();
            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
            var currYear = currentDate.Year;
            for (int i  = 1958; i <= currYear; i++)
            {
                var y = 0;
                if (i < 2000)
                {
                    y = i - 1900;
                }
                else y = i - 2000;
                if (i  < 2021)
                {
                    await ExtractFromTxt(data, i, y);

                }
                else 
                { 
                    data.Add(i, new DrawsYear { Draws = Array.Empty<LotteryDraw>() });
                }
            }
            return data;
        }

        private async Task ExtractFromTxt(Dictionary<int, DrawsYear> data, int i, int y)
        {
            var response = await httpClient.GetAsync($"649_{y}.txt");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var draws = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                    .Skip(1)
                    .Select(line =>
                    {
                        var parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        return new LotteryDraw
                        {
                            DrawNumber = int.Parse(parts[0]),
                            Numbers = parts.Skip(1).Take(6).Select(int.Parse).ToArray()
                        };
                    }).ToArray();
                data.Add(i, new DrawsYear { Draws = draws });
            }
        }

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
