using Microsoft.AspNetCore.Mvc;
using PfeProjet.Services;
using System.Threading.Tasks;

namespace PfeProjet.Controllers
{
    [Route("api/agentpool")]
    [ApiController]
    public class AgentPoolController : ControllerBase { 
    
        private readonly AgentPoolsService _poolsService;

        // Injecting the service class here, not the controller
        public AgentPoolController(AgentPoolsService poolsService)
        {
            _poolsService = poolsService;
        }

        [HttpGet("get-AgentPool")]
        public async Task<IActionResult> GetAgentPool([FromQuery] string organisation, [FromQuery] string pat, [FromQuery] string project)
        {
            if (string.IsNullOrEmpty(organisation) || string.IsNullOrEmpty(pat) || string.IsNullOrEmpty(project))
            {
                return BadRequest("L'organisation et le PAT sont requis.");
            }

            var result = await _poolsService.GetAgentPoolAsync(organisation, pat, project);

            if (result.Contains("Erreur"))
            {
                return BadRequest(result);
            }

            return Ok(result);
        }


        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetAgentPoolById(
            [FromQuery] string organisation,
            [FromQuery] string pat,
            [FromQuery] string project,
            [FromQuery] int AgentPoolId)
        {
            if (string.IsNullOrEmpty(organisation) || string.IsNullOrEmpty(pat) || string.IsNullOrEmpty(project))
            {
                return BadRequest("Organisation, PAT, and project are required.");
            }

            var result = await _poolsService.GetAgentPoolByIdAsync(organisation, pat, project, AgentPoolId);

            // Return the response
            if (!result.Contains("Error"))
            {
                return Ok(result);
            }

            return BadRequest(result);
        }


    }
}
