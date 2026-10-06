using WeatherReport.Models;

namespace WeatherReport.Services;

public static class WeatherAnalyzer
{
    /// <summary>Группирует города по странам и считает количество, среднюю, мин. и макс. температуру.</summary>
    public static IReadOnlyList<CountryStats> GroupByCountry(IEnumerable<WeatherData> data) =>
        data.GroupBy(d => d.Country, StringComparer.OrdinalIgnoreCase)
            .Select(g => new CountryStats(
                Country: g.First().Country,
                CityCount: g.Count(),
                AverageC: g.Average(d => d.TemperatureC),
                MinC: g.Min(d => d.TemperatureC),
                MaxC: g.Max(d => d.TemperatureC)))
            .OrderByDescending(s => s.CityCount)
            .ThenBy(s => s.Country, StringComparer.OrdinalIgnoreCase)
            .ToList();
}
