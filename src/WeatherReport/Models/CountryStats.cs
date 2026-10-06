namespace WeatherReport.Models;

/// <summary>Сводка по одной стране.</summary>
public sealed record CountryStats(string Country, int CityCount, double AverageC, int MinC, int MaxC);
