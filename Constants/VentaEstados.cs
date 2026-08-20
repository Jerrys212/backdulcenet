namespace DulceAtardecer.Constants;

public static class VentaEstados
{
    public const string Pendiente = "Pendiente";
    public const string Pagada = "Pagada";
    public const string Cancelada = "Cancelada";

    public static IReadOnlyList<string> GetAll() => [Pendiente, Pagada, Cancelada];

    public static bool EsValido(string estado) => GetAll().Contains(estado);
}
