using MemeApi.library.Extensions;
using MemeApi.library.repositories;
using MemeApi.library.Services.Files;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.MSIdentity.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MemeApi.Controllers;


/// <summary>
/// A controller for creating memes made of visuals and textual components.
/// </summary>
[Route("[controller]")]
[ApiController]
public class MemesController : ControllerBase
{
    private readonly IMemeRenderingService _memeRendererService;
    private readonly MemeRepository _memeRepository;

    /// <summary>
    /// A controller for creating memes made of visuals and textual components.
    /// </summary>
    public MemesController(MemeRepository memeRepository, IMemeRenderingService memeRendererService)
    {
        _memeRepository = memeRepository;
        _memeRendererService = memeRendererService;
    }
    /// <summary>
    /// Get all memes
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemeDTO>>> GetMemes()
    {
        var memes = await _memeRepository.GetMemes();
        return Ok(memes.Select(m => m.ToMemeDTO()));
    }

    /// <summary>
    /// Get a specific meme by ID
    /// </summary> 
    [HttpGet("{id}")]
    public async Task<ActionResult<MemeDTO>> GetMeme(string id)
    {
        var meme = await _memeRepository.GetMeme(id);
        if (meme == null) return NotFound();

        return Ok(meme.ToMemeDTO());
    }

    //[HttpPut("{id}")]
    //public async Task<IActionResult> PutMeme(int id, Meme meme)
    //{
    //    if (id != meme.Id)
    //    {
    //        return BadRequest();
    //    }

    //    if (await _memeRepository.ModifyMeme(id, meme)) return NotFound();

    //    return NoContent();
    //}

    /// <summary>
    /// Create a meme
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<MemeDTO>> PostMeme([FromForm]MemeCreationDTO memeCreationDto)
    {
        if (!memeCreationDto.VisualFile.FileName.Equals("VisualFile")) memeCreationDto.FileName = memeCreationDto.VisualFile.FileName;
        var meme = await _memeRepository.CreateMeme(memeCreationDto);
        if (meme == null) return NotFound("One of the topics was not found");
        return CreatedAtAction(nameof(GetMeme), new { id = meme.Id }, meme.ToMemeDTO());
    }

    /// <summary>
    /// Delete a meme
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMeme(string id)
    {
        if (await _memeRepository.DeleteMeme(id)) return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Get a random meme based on an optional seed
    /// </summary>
    [HttpGet]
    [Route("random/{seed?}")]
    public async Task<ActionResult<Meme>> RandomMeme(string seed = "")
    {
        var list = await _memeRepository.GetMemes();
        var regex = new Regex("^.*\\.gif$");
        list = list
            .Where(x => !regex.IsMatch(x.MemeVisual.Filename))
            .Where(x => x.Toptext == null || x.Toptext.Text.Length < 150)
            .Where(x => x.BottomText == null || x.BottomText.Text.Length < 150)
            .ToList();
        return Ok(list.RandomItem(seed));
    }

    /// <summary>
    /// Get a random meme rendered to a png in the response
    /// Use the optional Query parameters TopText and BottomText to define what the top and bottom text should be
    /// </summary>
    [HttpGet("random/Rendered")]
    public async Task<ActionResult> RenderImage([FromQuery] string TopText = null, [FromQuery] string BottomText = null)
    {
        var meme = await _memeRepository.RandomMemeByComponents(TopText, BottomText);
        var jsonResponse = meme.ToMemeDTO();

        Response.Headers.Add("X-File-Info", JsonConvert.SerializeObject(jsonResponse));

        return File(_memeRendererService.RenderMeme(meme), "image/png");
    }
}

