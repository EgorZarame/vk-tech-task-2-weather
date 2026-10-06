# WeatherReport

Консольное приложение на C# (.NET 8). Оно читает список городов из файла, получает для каждого текущую погоду через [wttr.in](https://wttr.in) и выводит сводку по городам и странам.

Из внешних зависимостей только `System.Text.Json`, входящий в .NET. Сторонних NuGet-пакетов нет.

## Запуск

Нужен [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
dotnet run --project src/WeatherReport
```

По умолчанию используется [`cities.txt`](cities.txt) из корня репозитория. Свой файл можно передать аргументом:

```bash
dotnet run --project src/WeatherReport -- path/to/cities.txt
```

Формат файла: один город на строку. Пустые строки и строки, начинающиеся с `#`, пропускаются.

## Пример вывода

```
Погода по городам:
  Moscow, Russia +13 °C
  Khabarovsk, Russia +8 °C
  Saint-Petersburg, Russia +12 °C
  Vienna, Austria +21 °C
  Izhevsk, Russia +4 °C
  Perm, Russia +2 °C
  NhaTrang, Vietnam +25 °C
  Villach, Austria +4 °C

По странам:
  Russia — 5 cities, avg: +7.8 °C, min: +2 °C, max: +13 °C
  Austria — 2 cities, avg: +12.5 °C, min: +4 °C, max: +21 °C
  Vietnam — 1 city, avg: +25 °C, min: +25 °C, max: +25 °C
```

## Как это работает

1. **Загрузка списка** ([`CityListLoader`](src/WeatherReport/Services/CityListLoader.cs)). Файл читается в память, строки обрезаются, дубликаты убираются без учёта регистра (`Moscow` и `moscow` считаются одним городом). Порядок первого появления сохраняется.
2. **Запросы к API** ([`WttrClient`](src/WeatherReport/Services/WttrClient.cs)). Для каждого уникального города выполняется `GET https://wttr.in/{City}?format=j1`. Запросы идут параллельно, результаты выводятся в порядке файла. При сетевых ошибках, таймаутах и ответах 5xx делается до 3 попыток.
3. **Модель** [`WeatherData`](src/WeatherReport/Models/WeatherData.cs) содержит поля `City`, `TemperatureC` и `Country`. Температура берётся из `current_condition[0].temp_C`, страна из `nearest_area[0].country[0].value`. В `City` записывается название из исходного файла, потому что `areaName` в ответе wttr.in бывает локализованным («Москва») или указывает на ближайший район («Лахта» для Saint-Petersburg).
4. **Вывод по городам** в формате `Tokyo, Japan +18 °C`.
5. **Группировка по странам** ([`WeatherAnalyzer`](src/WeatherReport/Services/WeatherAnalyzer.cs)). Для каждой страны считаются количество городов, средняя, минимальная и максимальная температура. Средняя округляется до десятых. Страны отсортированы по числу городов.

Если по какому-то городу данные получить не удалось, в выводе будет строка с ошибкой, а статистика посчитается по остальным городам.

| Код возврата | Значение |
|---|---|
| `0` | данные получены по всем городам |
| `1` | файл не найден или пуст |
| `2` | не удалось получить данные ни по одному городу |
| `3` | данные получены частично |

## Структура

```
cities.txt                       список городов
src/WeatherReport/
  Program.cs                     точка входа: загрузка → запросы → вывод
  Formatting.cs                  форматирование строк вывода
  Models/WeatherData.cs          модель погоды по городу
  Models/CountryStats.cs         сводка по стране
  Services/CityListLoader.cs     чтение и дедупликация списка
  Services/WttrClient.cs         запрос и разбор JSON wttr.in
  Services/WeatherAnalyzer.cs    группировка по странам
```
