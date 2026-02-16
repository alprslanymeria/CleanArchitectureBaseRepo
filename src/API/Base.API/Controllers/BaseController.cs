using Base.Application.Common;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Base.API.Controllers;

[ApiController]
public class BaseController : ControllerBase
{
    [NonAction]
    public IActionResult ActionResultInstance<T>(ServiceResult<T> response) where T : class
    {
        return response.Status switch
        {
            HttpStatusCode.NoContent => NoContent(),
            HttpStatusCode.Created => Created(response.UrlAsCreated, response),
            _ => new ObjectResult(response) { StatusCode = response.Status.GetHashCode() }
        };
    }

    [NonAction]
    public IActionResult ActionResultInstance(ServiceResult response)
    {
        return response.Status switch
        {
            HttpStatusCode.NoContent => new ObjectResult(null) { StatusCode = response.Status.GetHashCode() },
            _ => new ObjectResult(response) { StatusCode = response.Status.GetHashCode() }
        };
    }
}
