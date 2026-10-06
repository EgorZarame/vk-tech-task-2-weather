namespace WeatherReport.Models;

/// <summary>Текущая погода в одном городе.</summary>
/// <param name="City">Город (как указан в исходном списке).</param>
/// <param name="TemperatureC">Текущая температура, °C.</param>
/// <param name="Country">Страна, которую вернул API.</param>
public sealed record WeatherData(string City, int TemperatureC, string Country);
