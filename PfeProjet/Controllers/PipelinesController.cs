using Microsoft.AspNetCore.Mvc;
using PfeProjet.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace PfeProjet.Controllers
{
    [Route("api/pipelines")]
    [ApiController]
    public class PipelinesController : ControllerBase
    {
        private readonly PipelinesService _pipelinesService;
        public PipelinesController(PipelinesService pipelinesService)
        {
            _pipelinesService = pipelinesService;

        }

        [HttpGet("get-piplines")]
        public async Task<IActionResult> GetPiplines([FromQuery] string organisation, [FromQuery] string pat, [FromQuery] string project)
        {
            if (string.IsNullOrEmpty(organisation) || string.IsNullOrEmpty(pat) || string.IsNullOrEmpty(project))
            {
                return BadRequest("L'organisation et le PAT sont requis.");
            }

            var result = await _pipelinesService.GetPipelinesAsync(organisation, pat, project);

            if (result.Contains("Erreur"))
            {
                return BadRequest(result);
            }

            return Ok(result);
        }



    }
}
