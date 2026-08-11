using Microsoft.AspNetCore.Mvc;

namespace CoreFinance.Api.Controllers.Base;

/// <summary>
///     Base controller for API controllers, provides common functionality. (EN)<br />
///     Controller cơ sở cho các API controller, cung cấp các chức năng chung. (VI)
/// </summary>
[Route("api/[controller]")]
[ApiController]
public abstract class BaseController(
    ILogger logger
) : ControllerBase
{
    public readonly ILogger Logger = logger;
}