using System.Text;
using WeatherReport;
using WeatherReport.Models;
using WeatherReport.Services;

Console.OutputEncoding = Encoding.UTF8;

// Путь к списку городов: первый аргумент, иначе cities.txt в текущей папке, иначе рядом с exe.
var path = args.Length > 0
    ? args[0]
    : File.Exists("cities.txt") ? "cities.txt" : Path.Combine(AppContext.BaseDirectory, "cities.txt");

// 1. Загружаем уникальные города.
IReadOnlyList<string> cities;
try
{
    cities = CityListLoader.Load(path);
}
catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

if (cities.Count == 0)
{
    Console.Error.WriteLine($"В файле {path} нет ни одного города.");
    return 1;
}

// 2. Запрашиваем погоду (параллельно, порядок результатов = порядок в файле).
using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
http.DefaultRequestHeaders.UserAgent.ParseAdd("curl/8.0"); // wttr.in отдаёт JSON любому клиенту, но так стабильнее
var client = new WttrClient(http);

var tasks = cities.Select(async city =>
{
    try { return (city, data: await client.GetAsync(city), error: (string?)null); }
    catch (WeatherException ex) { return (city, data: (WeatherData?)null, error: ex.Message); }
}).ToList();
var results = await Task.WhenAll(tasks);

// 3–4. Выводим данные по каждому городу.
Console.WriteLine("Погода по городам:");
foreach (var (city, data, error) in results)
    Console.WriteLine(data is not null ? "  " + Formatting.City(data) : $"  {city}: ошибка — {error}");

var weather = results.Where(r => r.data is not null).Select(r => r.data!).ToList();
if (weather.Count == 0)
{
    Console.Error.WriteLine("Не удалось получить данные ни по одному городу.");
    return 2;
}

// 5. Группировка по странам.
Console.WriteLine();
Console.WriteLine("По странам:");
foreach (var stats in WeatherAnalyzer.GroupByCountry(weather))
    Console.WriteLine("  " + Formatting.Country(stats));

return weather.Count == cities.Count ? 0 : 3;
