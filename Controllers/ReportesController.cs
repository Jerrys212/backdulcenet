using Asp.Versioning;
using DulceAtardecer.Models.Dtos.Reportes;
using DulceAtardecer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DulceAtardecer.Controllers;

[ApiController]
[ApiVersionNeutral]
[Route("api/reportes")]
[Authorize(Roles = "Admin")]
public class ReportesController(IReportesService reportesService) : ControllerBase
{
    [HttpGet("daily")]
    [ProducesResponseType(typeof(DailyReportResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DailyReportResponseDto>> GetDailyAsync(CancellationToken cancellationToken)
    {
        return Ok(await reportesService.GetDailyAsync(cancellationToken));
    }

    [HttpPost("date-range")]
    [ProducesResponseType(typeof(DateRangeReportResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DateRangeReportResponseDto>> GetDateRangeAsync(DateRangeDto dto, CancellationToken cancellationToken)
    {
        return Ok(await reportesService.GetDateRangeAsync(dto, cancellationToken));
    }

    [HttpPost("top-products")]
    [ProducesResponseType(typeof(TopProductsReportResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TopProductsReportResponseDto>> GetTopProductsAsync(TopProductsQueryDto dto, CancellationToken cancellationToken)
    {
        return Ok(await reportesService.GetTopProductsAsync(dto, cancellationToken));
    }

    [HttpPost("categories")]
    [ProducesResponseType(typeof(IEnumerable<CategoryPerformanceItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<CategoryPerformanceItemDto>>> GetCategoryPerformanceAsync(
        OptionalDateRangeDto dto, CancellationToken cancellationToken)
    {
        return Ok(await reportesService.GetCategoryPerformanceAsync(dto, cancellationToken));
    }

    [HttpPost("users")]
    [ProducesResponseType(typeof(IEnumerable<UserPerformanceItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<UserPerformanceItemDto>>> GetUserPerformanceAsync(
        OptionalDateRangeDto dto, CancellationToken cancellationToken)
    {
        return Ok(await reportesService.GetUserPerformanceAsync(dto, cancellationToken));
    }
}
