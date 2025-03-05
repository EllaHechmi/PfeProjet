using Microsoft.AspNetCore.Mvc;
using PfeProjet.Models;
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
        [HttpGet("get-releases-fromDB")]
        public async Task<ActionResult<List<Release>>> GetAllReleasesFromMongo()
        {
            var releases = await _releasesService.GetAllReleasesFromMongoAsync();

            if (releases == null || releases.Count == 0)
            {
                return NotFound("No releases found in the database.");
            }

            return Ok(releases);
        }
        //get release fom db by id
        [HttpGet("{id}")]
        public async Task<ActionResult<Release>> GetReleaseById(int id)
        {
            var release = await _releasesService.GetReleaseByIdFromDbAsync(id);

            if (release == null)
            {
                return NotFound($"Release with ID {id} not found.");
            }

            return Ok(release);
        }

    }
}
