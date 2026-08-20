using Asp.Versioning;
using DulceAtardecer.Common.Authorization;
using DulceAtardecer.Constants;
using DulceAtardecer.Models.Dtos.Reporte;
using DulceAtardecer.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DulceAtardecer.Controllers;

[ApiController]
[ApiVersionNeutral]
[Route("api/[controller]")]
[Authorize]
[HasPermission(Permissions.Reportes.Read)]
public class ReportesController(IReporteRepository reporteRepository) : ControllerBase
{
    [HttpGet("daily")]
    [ProducesResponseType(typeof(DailyReporteDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DailyReporteDto>> GetDailyAsync(CancellationToken cancellationToken)
    {
        DailyReporteDto result = await reporteRepository.GetDailyAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("date-range")]
    [ProducesResponseType(typeof(DateRangeReporteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DateRangeReporteDto>> GetDateRangeAsync(
        [FromQuery] DateRangeRequestDto request, CancellationToken cancellationToken)
    {
        // StartDate/EndDate ya vienen garantizados no-nulos acá: DateRangeRequestDtoValidator
        // los exige antes de que la acción se ejecute (ValidationFilter global).
        DateRangeReporteDto result = await reporteRepository.GetDateRangeAsync(
            request.StartDate!.Value, request.EndDate!.Value, cancellationToken);
        return Ok(result);
    }

    [HttpGet("top-products")]
    [ProducesResponseType(typeof(TopProductsReporteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TopProductsReporteDto>> GetTopProductsAsync(
        [FromQuery] TopProductsRequestDto request, CancellationToken cancellationToken)
    {
        TopProductsReporteDto result = await reporteRepository.GetTopProductsAsync(
            request.StartDate, request.EndDate, request.Limit ?? 10, cancellationToken);
        return Ok(result);
    }

    [HttpGet("categories")]
    [ProducesResponseType(typeof(IEnumerable<CategoryPerformanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<CategoryPerformanceDto>>> GetCategoryPerformanceAsync(
        [FromQuery] OptionalDateRangeRequestDto request, CancellationToken cancellationToken)
    {
        IEnumerable<CategoryPerformanceDto> result = await reporteRepository.GetCategoryPerformanceAsync(
            request.StartDate, request.EndDate, cancellationToken);
        return Ok(result);
    }

    [HttpGet("users")]
    [ProducesResponseType(typeof(IEnumerable<UserPerformanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<UserPerformanceDto>>> GetUserPerformanceAsync(
        [FromQuery] OptionalDateRangeRequestDto request, CancellationToken cancellationToken)
    {
        IEnumerable<UserPerformanceDto> result = await reporteRepository.GetUserPerformanceAsync(
            request.StartDate, request.EndDate, cancellationToken);
        return Ok(result);
    }
}
