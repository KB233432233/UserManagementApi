using Microsoft.AspNetCore.Mvc;

namespace UserManagement.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class ErrorController : ControllerBase
{
    [Route("/error")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Error()
    {
        return Problem(detail: "An unexpected error occurred.");
    }
}