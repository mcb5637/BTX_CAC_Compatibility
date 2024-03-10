using System.Globalization;



CultureInfo i = new CultureInfo("en-us");
CultureInfo.DefaultThreadCurrentCulture = i;
CultureInfo.DefaultThreadCurrentUICulture = i;
while (true)
{
    string? l = Console.ReadLine();
    if (l == null || l == "" || l == "exit")
        break;
    if (l.StartsWith("jam"))
    {
        l = l[3..];
        string[] a = l.Trim().Split('-');
        if (a.Length != 2)
        {
            Console.WriteLine("error, missing min-max");
            continue;
        }
        int b = 10;
        double min = double.Parse(a[0]);
        double max = double.Parse(a[1]);
        Console.WriteLine($"\"FlatJammingChance\": {min},");
        Console.WriteLine($"\"GunneryJammingBase\": {b},");
        Console.WriteLine($"\"GunneryJammingMult\": {(max-min)/b},");
    }
}
