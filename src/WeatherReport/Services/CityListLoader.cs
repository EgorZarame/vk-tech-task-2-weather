namespace WeatherReport.Services;

public static class CityListLoader
{
    /// <summary>
    /// Читает файл (один город на строку) и возвращает уникальные названия
    /// в порядке первого появления. Пустые строки и строки с '#' пропускаются,
    /// дубликаты сравниваются без учёта регистра.
    /// </summary>
    public static IReadOnlyList<string> Load(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Файл со списком городов не найден: {path}", path);

        return Parse(File.ReadAllLines(path));
    }

    public static IReadOnlyList<string> Parse(IEnumerable<string> lines) =>
        lines.Select(l => l.Trim().TrimStart('﻿'))
             .Where(l => l.Length > 0 && !l.StartsWith('#'))
             .Distinct(StringComparer.OrdinalIgnoreCase)
             .ToList();
}
