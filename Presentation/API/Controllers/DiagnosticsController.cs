using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

// Manual-verification tool for the logging pipeline, not a real business endpoint — see LogTestService.
[ApiController]
[Route("api/[controller]/[action]")]
public class DiagnosticsController(ILogTestService logTestService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> LogTest([FromQuery] bool fail = false)
    {
        await logTestService.RunAsync(fail);
        return Ok();
    }
}
