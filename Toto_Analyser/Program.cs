using Toto_Analyser;

public class Program
{
    public static void Main(string[] args)
    {
        DataLoader data = new DataLoader();
        var result = data.LoadData().Result;
        foreach (var item in result)
        {
            Console.WriteLine($"Year: {item.Key}");
            foreach (var draw in item.Value.Draws)
            {
                Console.WriteLine($"Draw: {string.Join(", ", draw)}");
            }
        }
    }
}