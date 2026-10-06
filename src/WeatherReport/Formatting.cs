using System.Globalization;
using WeatherReport.Models;

namespace WeatherReport;

public static class Formatting
{
    /// <summary>Температура со знаком: +18 °C, -3 °C, 0 °C. Среднее — до десятых: +4.7 °C.</summary>
    public static string Temp(double celsius)
    {
        var rounded = Math.Round(celsius, 1, MidpointRounding.AwayFromZero);
        return rounded.ToString("+0.#;-0.#;0", CultureInfo.InvariantCulture) + " °C";
    }

    /// <summary>Tokyo, Japan +18 °C</summary>
    public static string City(WeatherData d) => $"{d.City}, {d.Country} {Temp(d.TemperatureC)}";

    /// <summary>Japan — 3 cities, avg: +19 °C, min: +16 °C, max: +22 °C</summary>
    public static string Country(CountryStats s) =>
        $"{s.Country} — {s.CityCount} {(s.CityCount == 1 ? "city" : "cities")}, " +
        $"avg: {Temp(s.AverageC)}, min: {Temp(s.MinC)}, max: {Temp(s.MaxC)}";
}
