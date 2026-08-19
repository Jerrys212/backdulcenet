using DulceAtardecer.Models;
using DulceAtardecer.Models.Dtos.Categoria;
using DulceAtardecer.Models.Dtos.Producto;
using Mapster;

namespace DulceAtardecer.Mapping;

public static class MapsterConfig
{
    public static void RegisterMappings()
    {
        TypeAdapterConfig<Categoria, CategoriaDto>.NewConfig();

        TypeAdapterConfig<CreateCategoriaDto, Categoria>.NewConfig();

        TypeAdapterConfig<UpdateCategoriaDto, Categoria>.NewConfig();

        TypeAdapterConfig<Producto, ProductoDto>.NewConfig()
            .Map(dest => dest.CategoriaNombre, src => src.Categoria != null ? src.Categoria.Nombre : string.Empty);

        TypeAdapterConfig<CreateProductoDto, Producto>.NewConfig()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.ImgUrl)
            .Ignore(dest => dest.ImgUrlLocal);

        TypeAdapterConfig<UpdateProductoDto, Producto>.NewConfig()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.ImgUrl)
            .Ignore(dest => dest.ImgUrlLocal);
    }
}
