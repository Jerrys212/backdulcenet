using DulceAtardecer.Models;
using DulceAtardecer.Models.Dtos.Categoria;
using DulceAtardecer.Models.Dtos.Extra;
using DulceAtardecer.Models.Dtos.Producto;
using DulceAtardecer.Models.Dtos.SubCategoria;
using DulceAtardecer.Models.Dtos.Venta;
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
            .Map(dest => dest.CategoriaNombre, src => src.Categoria != null ? src.Categoria.Nombre : string.Empty)
            .Map(dest => dest.SubCategoriaNombre, src => src.SubCategoria != null ? src.SubCategoria.Nombre : string.Empty);

        TypeAdapterConfig<CreateProductoDto, Producto>.NewConfig()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.ImgUrl)
            .Ignore(dest => dest.ImgUrlLocal)
            .Ignore(dest => dest.Activo)
            .Ignore(dest => dest.FechaCreacion)
            .Ignore(dest => dest.FechaActualizacion);

        TypeAdapterConfig<UpdateProductoDto, Producto>.NewConfig()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.ImgUrl)
            .Ignore(dest => dest.ImgUrlLocal)
            .Ignore(dest => dest.Activo)
            .Ignore(dest => dest.FechaCreacion)
            .Ignore(dest => dest.FechaActualizacion);

        TypeAdapterConfig<Extra, ExtraDto>.NewConfig();

        TypeAdapterConfig<CreateExtraDto, Extra>.NewConfig()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Activo)
            .Ignore(dest => dest.FechaCreacion)
            .Ignore(dest => dest.FechaActualizacion);

        TypeAdapterConfig<UpdateExtraDto, Extra>.NewConfig()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Activo)
            .Ignore(dest => dest.FechaCreacion)
            .Ignore(dest => dest.FechaActualizacion);

        TypeAdapterConfig<VentaItem, VentaItemDto>.NewConfig()
            .Map(dest => dest.Extras, src => src.Extras.Select(e =>
                new VentaItemExtraDto(e.Extra!.Id, e.Extra.Nombre, e.Precio)));

        TypeAdapterConfig<Venta, VentaDto>.NewConfig()
            .Map(dest => dest.SellerNombre, src => src.Seller != null ? src.Seller.Nombre : string.Empty)
            .Map(dest => dest.EstadoActualizadoPorNombre, src => src.EstadoActualizadoPor != null ? src.EstadoActualizadoPor.Nombre : string.Empty);
    }
}
