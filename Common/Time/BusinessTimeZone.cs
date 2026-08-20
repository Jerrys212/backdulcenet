using NodaTime;

namespace DulceAtardecer.Common.Time;

/// <summary>
/// Zona horaria de negocio fija (America/Mexico_City), independiente de donde corra el servidor.
/// Portado de business-timezone.util.ts (módulo Reportes de la versión NestJS).
/// </summary>
public static class BusinessTimeZone
{
    private static readonly DateTimeZone Zone = DateTimeZoneProviders.Tzdb["America/Mexico_City"];

    public static (DateTime StartUtc, DateTime EndUtc) GetBusinessDayRange(DateTime? reference = null)
    {
        Instant instant = ToInstant(reference ?? DateTime.UtcNow);
        LocalDate localDate = instant.InZone(Zone).Date;

        Instant startInstant = Zone.AtLeniently(localDate.AtMidnight()).ToInstant();
        Instant endInstant = Zone.AtLeniently(localDate.AtMidnight().PlusHours(23).PlusMinutes(59).PlusSeconds(59).PlusMilliseconds(999)).ToInstant();

        return (startInstant.ToDateTimeUtc(), endInstant.ToDateTimeUtc());
    }

    public static string ToBusinessDateKey(DateTime dateTimeUtc)
    {
        LocalDate localDate = ToInstant(dateTimeUtc).InZone(Zone).Date;
        return localDate.ToString("yyyy-MM-dd", null);
    }

    private static Instant ToInstant(DateTime dateTime)
    {
        DateTime utc = dateTime.Kind == DateTimeKind.Utc ? dateTime : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        return Instant.FromDateTimeUtc(utc);
    }
}
