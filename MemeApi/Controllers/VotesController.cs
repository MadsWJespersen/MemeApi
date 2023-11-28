using MemeApi.library.repositories;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MemeApi.Controllers;

/// <summary>
/// A controller for creating managing votes
/// </summary>
[Route("[controller]")]
[ApiController]
public class VotesController : ControllerBase
{
    private readonly VotableRepository _votableRepository;
    private readonly TextRepository _textRepository;
    private readonly VisualRepository _visualRepository;
    private readonly MemeRepository _memeRepository;
    private readonly UserRepository _userRepository;
    /// <summary>
    /// A controller for creating managing votes
    /// </summary>
    public VotesController(VotableRepository votableRepository, TextRepository textRepository, VisualRepository visualRepository, MemeRepository memeRepository, UserRepository userRepository)
    {
        _votableRepository = votableRepository;
        _textRepository = textRepository;
        _visualRepository = visualRepository;
        _memeRepository = memeRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Get all votes.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Vote>>> GetVotes()
    {
        return await _votableRepository.GetVotes();
    }

    /// <summary>
    /// Get one vote.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Vote>> GetVote(string id)
    {
        var vote = await _votableRepository.GetVote(id);

        if (vote == null)
        {
            return NotFound();
        }

        return vote;
    }

    /// <summary>
    /// Vote on a particular meme component or meme component.
    /// To vote on a component or existing meme include one id, the id of the component.
    /// To vote for a meme that does not exist yet, include the id's of elements it contains.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Vote>> PostVote([FromForm]VoteDTO voteDTO)
    {
        var components = await _votableRepository.FindMany(voteDTO.ElementIDs);
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdString == null || components.Count == 0)
        {
            return NotFound();
        }

        var userId = userIdString;
        Votable element;

        if (components.Count > 1)
        {
            var visual = await _visualRepository.GetVisual(components.SingleOrDefault(item => item is MemeVisual).Id);
            var texts = await Task.WhenAll(
                components.Where(item => item is MemeText)
                    .Select(async text => await _textRepository.GetText(text.Id))
            );

            if (texts.Length == 0) return NotFound();
            var toptext = texts.Length > 0 ? texts[0] : null;
            var bottomtext = texts.Length > 1 ? texts[1] : null;

            element = await _memeRepository.UpsertByComponents(visual, toptext, bottomtext);
        }
        else
        {
            element = components[0];
        }

        Vote vote;
        var existingVote = _votableRepository.FindByElementAndUser(element, userId);

        if (existingVote != null)
        {
            if (voteDTO.UpVote != Upvote.Unvote)
            { 
                return await _votableRepository.ChangeVote(existingVote, voteDTO.UpVote);
            }
            else
            {
                return await DeleteVote(existingVote.Id);
            }
        }
        else
        {
            if (voteDTO.UpVote == Upvote.Unvote)
            {
                return BadRequest("Can't unvote because no vote exists");
            }
            vote = new Vote
            {
                Id = Guid.NewGuid().ToString(),
                Upvote = voteDTO.UpVote == Upvote.Upvote,
                Element = element,
                User = await _userRepository.GetUser(userId),
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
            };

            await _votableRepository.CreateVote(vote);
        }

        return CreatedAtAction("GetVote", new { id = vote.Id }, vote);
    }

    /// <summary>
    /// Delete a vote. This can also be done with normal vote with Upvote = null
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteVote(string id)
    {
        var deleted = await _votableRepository.DeleteVote(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}
