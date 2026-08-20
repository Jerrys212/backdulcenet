using System.Globalization;

namespace DulceAtardecer.Common.Utils;

/// <summary>
/// Zona horaria fija del negocio (America/Mexico_City), independiente de dónde corra el servidor.
/// Ver docs/reportes.md §4 — usada solo por el módulo de Reportes.
/// </summary>
public static class BusinessTimeZone
{
    private static readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");

    public static (DateTime StartUtc, DateTime EndUtc) GetBusinessDayRange(DateTime referenceUtc)
    {
        DateTime localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(referenceUtc, DateTimeKind.Utc), TimeZone);
        DateTime localStart = DateTime.SpecifyKind(localNow.Date, DateTimeKind.Unspecified);
        DateTime localEnd = localStart.AddDays(1).AddMilliseconds(-1);

        DateTime startUtc = TimeZoneInfo.ConvertTimeToUtc(localStart, TimeZone);
        DateTime endUtc = TimeZoneInfo.ConvertTimeToUtc(localEnd, TimeZone);

        return (startUtc, endUtc);
    }

    public static string ToBusinessDateKey(DateTime utcDate)
    {
        DateTime local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcDate, DateTimeKind.Utc), TimeZone);
        return local.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}
