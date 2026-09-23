using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/jobs")]
    [ApiController]
    public class JobController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create()
        {
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetJobById()
        {
            return Ok();
        }
    }
}
