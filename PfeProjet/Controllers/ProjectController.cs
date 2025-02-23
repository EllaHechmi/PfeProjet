using Microsoft.AspNetCore.Mvc;
using PfeProjet.Models;
using PfeProjet.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace PfeProjet.Controllers
{

    [Route("api/projects")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly ProjectService _projectService;

        public ProjectController(ProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet("get-projects")]
        public async Task<IActionResult> GetProjects([FromQuery] string organisation, [FromQuery] string pat)
        {
            if (string.IsNullOrEmpty(organisation) || string.IsNullOrEmpty(pat))
            {
                return BadRequest("L'organisation et le PAT sont requis.");
            }

            var result = await _projectService.GetProjectAsync(organisation, pat);

            if (result.Contains("Erreur"))
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

   

    }
}