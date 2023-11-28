using MemeApi.library.Extensions;
using MemeApi.library.repositories;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MemeApi.Controllers;

/// <summary>
/// A controller for creating and managing visual meme components.
/// </summary>
[Route("[controller]")]
[ApiController]
public class VisualsController : ControllerBase
{
    private readonly VisualRepository _visualRepository;
    private readonly IConfiguration _configuration;
    /// <summary>
    /// A controller for creating and managing visual meme components.
    /// </summary>
    public VisualsController(VisualRepository visualRepository, IConfiguration configuration)
    {
        _visualRepository = visualRepository;
        _configuration = configuration;
    }

    /// <summary>
    /// Get all visuals
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VisualDTO>>> GetVisuals(){
        var visuals = await _visualRepository.GetVisuals();
        return Ok(visuals.Select(v => v.ToVisualDTO()));
    }
    /// <summary>
    /// Get a specific visual by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<MemeComponentDTO>> GetMemeVisual(string id)
    {
        var memeVisual = await _visualRepository.GetVisual(id);

        if (memeVisual == null) return NotFound();
        return Ok(memeVisual.ToRandomComponentDTO(_configuration["Media.Host"]));
    }

    /// <summary>
    /// Create a visual
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MemeVisual>> PostMemeVisual(IFormFile visual)
    {
        if (visual.Length > 5000000) return StatusCode(413);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var memeVisual = await _visualRepository.CreateMemeVisual(visual, visual.FileName, userId:userId);
        return CreatedAtAction("GetMemeVisual", new { id = memeVisual.Id }, memeVisual);
    }

    /// <summary>
    /// Delete a visual by ID
    /// </summary>

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMemeVisual(string id)
    {
        var deleted = await _visualRepository.RemoveMemeVisual(id);
        if (deleted == false)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Get a random visual
    /// </summary>
    [HttpGet]
    [Route("random")]
    public async Task<ActionResult<RandomComponentDTO>> RandomVisual()
    {
        var visual = (await _visualRepository.GetVisuals()).RandomItem();
        return Ok(visual.ToRandomComponentDTO(_configuration["Media.Host"]));
    }
}
