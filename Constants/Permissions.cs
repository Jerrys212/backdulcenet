using System.Reflection;

namespace DulceAtardecer.Constants;

public static class Permissions
{
    public static class Categorias
    {
        public const string Create = "categorias.create";
        public const string Read = "categorias.read";
        public const string Update = "categorias.update";
        public const string Delete = "categorias.delete";
    }

    public static class SubCategorias
    {
        public const string Create = "subcategorias.create";
        public const string Read = "subcategorias.read";
        public const string Update = "subcategorias.update";
        public const string Delete = "subcategorias.delete";
    }

    public static class Productos
    {
        public const string Create = "productos.create";
        public const string Read = "productos.read";
        public const string Update = "productos.update";
        public const string Delete = "productos.delete";
    }

    public static class Extras
    {
        public const string Create = "extras.create";
        public const string Read = "extras.read";
        public const string Update = "extras.update";
        public const string Delete = "extras.delete";
    }

    public static class Ventas
    {
        public const string Create = "ventas.create";
        public const string Read = "ventas.read";
        public const string Update = "ventas.update";
        public const string Delete = "ventas.delete";
    }

    public static IReadOnlyList<string> GetAll()
    {
        return typeof(Permissions)
            .GetNestedTypes(BindingFlags.Public | BindingFlags.Static)
            .SelectMany(module => module.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            .Where(field => field.IsLiteral && field.FieldType == typeof(string))
            .Select(field => (string)field.GetRawConstantValue()!)
            .ToList();
    }
}
