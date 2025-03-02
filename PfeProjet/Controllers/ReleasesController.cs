using Microsoft.AspNetCore.Mvc;
using PfeProjet.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace PfeProjet.Controllers
{
    [Route("api/releases")]

    [ApiController]
    public class ReleasesController : ControllerBase
    {
        private readonly ReleasesService _releasesService;
        public ReleasesController(ReleasesService releasesService)
        {
            _releasesService = releasesService;

        }

        [HttpGet("get-releases")]
        public async Task<IActionResult> GetReleases([FromQuery] string organisation, [FromQuery] string pat, [FromQuery] string project)
        {
            if (string.IsNullOrEmpty(organisation) || string.IsNullOrEmpty(pat) || string.IsNullOrEmpty(project))
            {
                return BadRequest("L'organisation et le PAT sont requis.");
            }

            var result = await _releasesService.GetReleasesAsync(organisation, pat, project);

            if (result.Contains("Erreur"))
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("get-by-id")]
         public async Task<IActionResult> GetReleasesById(
            [FromQuery] string organisation,
            [FromQuery] string pat,
            [FromQuery] string project,
            [FromQuery] int releasesId)
{
            if (string.IsNullOrEmpty(organisation) || string.IsNullOrEmpty(pat) || string.IsNullOrEmpty(project))
    {
                return BadRequest("Organisation, PAT, and project are required.");
    }

             var result = await _releasesService.GetReleasesByIdAsync(organisation, pat, project, releasesId );

    // Return the response
            if (!result.Contains("Error"))
    {
                  return Ok(result);
    }

             
            return BadRequest(result);
}


    }
}
