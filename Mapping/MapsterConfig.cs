using DulceAtardecer.Models;
using DulceAtardecer.Models.Dtos.Categoria;
using DulceAtardecer.Models.Dtos.Producto;
using DulceAtardecer.Models.Dtos.SubCategoria;
using Mapster;

namespace DulceAtardecer.Mapping;

public static class MapsterConfig
{
    public static void RegisterMappings()
    {
        TypeAdapterConfig<Categoria, CategoriaDto>.NewConfig();

        TypeAdapterConfig<CreateCategoriaDto, Categoria>.NewConfig()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Activo)
            .Ignore(dest => dest.FechaCreacion)
            .Ignore(dest => dest.FechaActualizacion);

        TypeAdapterConfig<UpdateCategoriaDto, Categoria>.NewConfig()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Activo)
            .Ignore(dest => dest.FechaCreacion)
            .Ignore(dest => dest.FechaActualizacion);

        TypeAdapterConfig<SubCategoria, SubCategoriaDto>.NewConfig()
            .Map(dest => dest.CategoriaNombre, src => src.Categoria != null ? src.Categoria.Nombre : string.Empty);

        TypeAdapterConfig<CreateSubCategoriaDto, SubCategoria>.NewConfig()
            .Ignore(dest => dest.Id);

        TypeAdapterConfig<UpdateSubCategoriaDto, SubCategoria>.NewConfig()
            .Ignore(dest => dest.Id);

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
