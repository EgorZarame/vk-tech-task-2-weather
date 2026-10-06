using System.Globalization;
using System.Net;
using System.Text.Json;
using WeatherReport.Models;

namespace WeatherReport.Services;

/// <summary>Клиент к https://wttr.in/{City}?format=j1.</summary>
public sealed class WttrClient
{
    private const int MaxAttempts = 3;

    private readonly HttpClient _http;

    public WttrClient(HttpClient http) => _http = http;

    public async Task<WeatherData> GetAsync(string city, CancellationToken ct = default)
    {
        var url = $"https://wttr.in/{Uri.EscapeDataString(city)}?format=j1";

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                using var response = await _http.GetAsync(url, ct);
                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new WeatherException($"город «{city}» не найден");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(ct);
                return Parse(city, json);
            }
            catch (Exception ex) when (attempt < MaxAttempts && IsTransient(ex, ct))
            {
                await Task.Delay(TimeSpan.FromSeconds(attempt), ct);
            }
            catch (Exception ex) when (ex is not WeatherException && IsTransient(ex, ct))
            {
                throw new WeatherException($"не удалось получить данные: {ex.Message}", ex);
            }
        }
    }

    /// <summary>Достаёт из ответа j1 текущую температуру и страну.</summary>
    public static WeatherData Parse(string city, string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var tempText = root.GetProperty("current_condition")[0].GetProperty("temp_C").GetString();
            if (!int.TryParse(tempText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var temp))
                throw new WeatherException($"некорректная температура в ответе: «{tempText}»");

            var country = root.GetProperty("nearest_area")[0]
                              .GetProperty("country")[0]
                              .GetProperty("value").GetString();
            if (string.IsNullOrWhiteSpace(country))
                throw new WeatherException("в ответе не указана страна");

            return new WeatherData(city, temp, country.Trim());
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException
                                       or IndexOutOfRangeException or InvalidOperationException)
        {
            throw new WeatherException($"неожиданный формат ответа ({ex.Message})", ex);
        }
    }

    private static bool IsTransient(Exception ex, CancellationToken ct) =>
        ex is HttpRequestException or WeatherException { InnerException: JsonException }
        || (ex is TaskCanceledException && !ct.IsCancellationRequested);
}

public sealed class WeatherException : Exception
{
    public WeatherException(string message, Exception? inner = null) : base(message, inner) { }
}
