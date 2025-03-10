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
        public class PipelineRequest
        {
            public string Organisation { get; set; }
            public string PersonalAccessToken { get; set; }
            public string Project { get; set; }
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


        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetPipelineById(
            [FromQuery] string organisation,
            [FromQuery] string pat,
            [FromQuery] string project,
            [FromQuery] int pipelineId)
        {
            if (string.IsNullOrEmpty(organisation) || string.IsNullOrEmpty(pat) || string.IsNullOrEmpty(project))
            {
                return BadRequest("Organisation, PAT, and project are required.");
            }

            var result = await _pipelinesService.GetPipelineByIdAsync(organisation, pat, project, pipelineId);

            // Return the response
            if (!result.Contains("Error"))
            {
                return Ok(result);
            }

            return BadRequest(result); 
        }

        // get pipelines by id from db 

        [HttpGet("{pipelineId}")]
        public async Task<IActionResult> GetPipelineByIdFromDb(int pipelineId)
        {
            var pipeline = await _pipelinesService.GetPipelineByIdFromDbAsync(pipelineId);
            if (pipeline == null)
            {
                return NotFound("Pipeline not found.");
            }
            return Ok(pipeline);
        }
        [HttpPost("fetch-tasks")]
        public async Task<IActionResult> FetchAndStoreAllPipelineTasks([FromBody] PipelineRequest request)
        {
            // Validate if the required parameters are provided
            if (string.IsNullOrEmpty(request.Organisation) ||
                string.IsNullOrEmpty(request.PersonalAccessToken) ||
                string.IsNullOrEmpty(request.Project))
            {
                return BadRequest("Missing required parameters.");
            }

            // Call the service method to fetch tasks and store them in MongoDB
            var result = await _pipelinesService.FetchAndStoreAllPipelineTasksAsync(request.Organisation, request.PersonalAccessToken, request.Project);

            // Return the result as an HTTP response
            return Ok(result);
        }


        [HttpGet("{pipelineId}/tasks")]
        public async Task<IActionResult> GetTasksByPipelineId(int pipelineId)
        {
            var tasks = await _pipelinesService.GetTasksByPipelineIdAsync(pipelineId);
            if (tasks == null || !tasks.Any())
            {
                return NotFound("Aucune tâche trouvée pour ce pipeline.");
            }
            return Ok(tasks);
        }



    }
}
