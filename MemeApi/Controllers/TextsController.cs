using MemeApi.library.Extensions;
using MemeApi.library.repositories;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MemeApi.Controllers;

/// <summary>
/// A controller for creating and managing meme textual components.
/// </summary>
[Route("[controller]")]
[ApiController]
public class TextsController : ControllerBase
{
    private readonly TextRepository _textRepository;
    /// <summary>
    /// A controller for creating and managing meme textual components.
    /// </summary>
    public TextsController(TextRepository textRepository)
    {
        _textRepository = textRepository;
    }

    /// <summary>
    /// Get all texts.
    /// Include text position to get all of that position
    /// </summary>
    [HttpGet("{type?}")]
    public async Task<ActionResult<IEnumerable<TextDTO>>> GetTexts(MemeTextPosition? type = null){
        var texts = await _textRepository.GetTexts(type);
        return Ok(texts.Select(t => t.ToTextDTO()));
    }

    /// <summary>
    /// Get a specific text by ID
    /// </summary>
    [HttpGet("one/{id}")]
    public async Task<ActionResult<TextDTO>> GetMemeText(string id)
    {
        var memeBottomText = await _textRepository.GetText(id);

        if (memeBottomText == null) return NotFound();

        return Ok(memeBottomText.ToTextDTO());
    }

    //[HttpPut("{id}")]
    //public async Task<IActionResult> UpdateMemeText(int id, string newMemeBottomText, MemeTextPosition? newMemeTextPosition = null)
    //{
    //    var memeText = await _textRepository.UpdateText(id, newMemeBottomText, newMemeTextPosition);

    //    if (!memeText) return NotFound();
    //    return NoContent();
    //}

    /// <summary>
    /// Create a meme text
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MemeText>> CreateMemeText([FromBody] TextCreationDTO textCreationDTO)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var memeText = await _textRepository.CreateText(textCreationDTO.Text, textCreationDTO.position, userId: userId);
        return CreatedAtAction("CreateMemeText", new { id = memeText.Id }, memeText);
    }

    /// <summary>
    /// Delete a meme text
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMemeText(string id)
    {
        var removed = await _textRepository.RemoveText(id);

        if (!removed) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Get a random meme text
    /// </summary>

    [HttpGet]
    [Route("random/{type}")]
    public async Task<ActionResult<RandomComponentDTO>> RandomText(MemeTextPosition type)
    {
        var texts = await _textRepository.GetTexts(type);
        var text = texts.RandomItem();
        return Ok(text.ToRandomComponentDTO());
    }

}
