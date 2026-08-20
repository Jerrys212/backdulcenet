namespace DulceAtardecer.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";

    public static IReadOnlyList<string> GetAll() => [Admin, User];

    public static bool EsValido(string role) => GetAll().Contains(role);
}
