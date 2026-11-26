using CrisisPlatform.Api.Common.Errors;
using CrisisPlatform.Api.Common.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace CrisisPlatform.Api.Modules.Requests;

/// <summary>
/// Thin HTTP adapter for aid requests.
/// All business logic lives in <see cref="RequestService"/>; this controller
/// only handles HTTP concerns (routing, status codes, header extraction).
/// </summary>
[ApiController]
[Route("api/v1/requests")]
public sealed class RequestsController(RequestService service) : ControllerBase
{
    /// <summary>Submit a new aid request.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(RequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Create(
        [FromBody] CreateRequestDto dto,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return BadRequest(new ProblemDetails
            {
                Title  = "Missing header",
                Detail = "An 'Idempotency-Key' header is required for this endpoint.",
                Status = StatusCodes.Status400BadRequest,
            });

        try
        {
            var result = service.Create(dto, idempotencyKey);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (IdempotencyConflictException ex)
        {
            return Conflict(new ProblemDetails { Title = "Idempotency conflict", Detail = ex.Message, Status = StatusCodes.Status409Conflict });
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Validation error", Detail = ex.Message, Status = 400 });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Validation error", Detail = ex.Message, Status = 400 });
        }
    }

    /// <summary>List requests with pagination and optional district filter.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<RequestDto>), StatusCodes.Status200OK)]
    public IActionResult GetAll(
        [FromQuery] string? district,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PaginationQuery.DefaultPageSize) =>
        Ok(service.ListPaged(new PaginationQuery(page, pageSize), district));

    /// <summary>Retrieve a single request by its unique ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var dto = service.GetById(id);
        return dto is null ? NotFound() : Ok(dto);
    }
}
