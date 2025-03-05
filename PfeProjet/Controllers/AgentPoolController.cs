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
        public async Task<IActionResult> GetAgentPool([FromQuery] string organisation, [FromQuery] string pat)
        {
            if (string.IsNullOrEmpty(organisation) || string.IsNullOrEmpty(pat))
            {
                return BadRequest("L'organisation et le PAT sont requis.");
            }

            var result = await _poolsService.GetAgentPoolAsync(organisation, pat);

            if (result.Contains("Erreur"))
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        //get all agent pools from mongo db
        [HttpGet("get-all-agent-pools")]
        public async Task<IActionResult> GetAllAgentPools()
        {
            try
            {
                // Fetch all agent pools from MongoDB
                var agentPools = await _poolsService.GetAllAgentPoolsFromMongoAsync();

                if (agentPools == null || agentPools.Count == 0)
                {
                    return NotFound("No agent pools found in MongoDB.");
                }

                // Return the list of agent pools
                return Ok(agentPools);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllAgentPools: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        /**  [HttpGet("get-by-id")]
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
        **/

    }
}
