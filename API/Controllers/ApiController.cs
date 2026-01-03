using Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class ApiController : ControllerBase
    {
        protected IActionResult HandleResult<T>(OperationResult<T> result)
        {
            if (result == null) return NotFound();
            
            if (result.IsSuccess && result.Data != null)
                return Ok(result.Data);
            
            if (result.IsSuccess && result.Data == null)
                return NotFound();
            
            if (result.Errors != null && result.Errors.Any())
                return BadRequest(result.Errors);
            
            return BadRequest(result.Message ?? "An error occurred");
        }

        protected IActionResult HandleResult(OperationResult result)
        {
            if (result == null) return NotFound();
            
            if (result.IsSuccess)
                return Ok(new { message = result.Message });
            
            if (result.Errors != null && result.Errors.Any())
                return BadRequest(result.Errors);
            
            return BadRequest(result.Message ?? "An error occurred");
        }
    }
}

