using Asp.Versioning;
using DulceAtardecer.Common.Responses;
using DulceAtardecer.Constants;
using DulceAtardecer.Models;
using DulceAtardecer.Models.Dtos.Producto;
using DulceAtardecer.Repository.IRepository;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DulceAtardecer.Controllers;

[ApiController]
[ApiVersionNeutral]
[Route("api/[controller]")]
[Authorize]
public class ProductosController(IProductoRepository productoRepository, IWebHostEnvironment webHostEnvironment)
    : ControllerBase
{
    private const string ImagesFolder = "ProductoImages";

    [HttpGet]
    [ResponseCache(CacheProfileName = CacheProfiles.Default10)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductoDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductoDto>>>> GetAllAsync(
        [FromQuery] int page = 1, [FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        (IEnumerable<Producto> items, int total) = await productoRepository.GetAllAsync(page, limit, cancellationToken);
        IEnumerable<ProductoDto> dtos = items.Adapt<IEnumerable<ProductoDto>>();
        return Ok(new ApiResponse<IEnumerable<ProductoDto>>(true, dtos, new ApiMeta(page, total)));
    }

    [HttpGet("{id:int}")]
    [ResponseCache(CacheProfileName = CacheProfiles.Default10)]
    [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        Producto producto = await productoRepository.GetByIdAsync(id, cancellationToken);
        return Ok(producto.Adapt<ProductoDto>());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductoDto>> CreateAsync([FromForm] CreateProductoDto createDto, CancellationToken cancellationToken)
    {
        Producto producto = createDto.Adapt<Producto>();
        producto.ImgUrl = "https://placehold.co/400x400?text=Sin+Imagen";
        producto = await productoRepository.CreateAsync(producto, cancellationToken);

        if (createDto.Imagen is not null)
        {
            (string imgUrl, string imgUrlLocal) = await SaveImagenAsync(producto.Id, createDto.Imagen);
            producto.ImgUrl = imgUrl;
            producto.ImgUrlLocal = imgUrlLocal;
            await productoRepository.UpdateAsync(producto.Id, producto, cancellationToken);
        }

        Producto productoCreado = await productoRepository.GetByIdAsync(producto.Id, cancellationToken);
        ProductoDto result = productoCreado.Adapt<ProductoDto>();
        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int id, [FromForm] UpdateProductoDto updateDto, CancellationToken cancellationToken)
    {
        Producto producto = updateDto.Adapt<Producto>();

        if (updateDto.Imagen is not null)
        {
            (string imgUrl, string imgUrlLocal) = await SaveImagenAsync(id, updateDto.Imagen);
            producto.ImgUrl = imgUrl;
            producto.ImgUrlLocal = imgUrlLocal;
        }

        await productoRepository.UpdateAsync(id, producto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await productoRepository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private async Task<(string ImgUrl, string ImgUrlLocal)> SaveImagenAsync(int productoId, IFormFile imagen)
    {
        string extension = Path.GetExtension(imagen.FileName);
        string fileName = $"{productoId}{Guid.NewGuid()}{extension}";
        string folderPath = Path.Combine(webHostEnvironment.WebRootPath, ImagesFolder);
        Directory.CreateDirectory(folderPath);
        string filePath = Path.Combine(folderPath, fileName);

        await using (FileStream stream = new(filePath, FileMode.Create))
        {
            await imagen.CopyToAsync(stream);
        }

        string imgUrl = $"{Request.Scheme}://{Request.Host}/{ImagesFolder}/{fileName}";
        string imgUrlLocal = filePath;
        return (imgUrl, imgUrlLocal);
    }
}
