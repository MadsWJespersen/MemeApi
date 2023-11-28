using MemeApi.library.Extensions;
using MemeApi.library.repositories;
using MemeApi.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MemeApi.Controllers;

/// <summary>
/// A controller for creating and managing meme and meme component groupings called topics.
/// </summary>
[Route("[controller]")]
[ApiController]
public class TopicsController : ControllerBase
{
    private readonly UserRepository _userRepository;
    private readonly TopicRepository _topicRepository;
    private readonly VotableRepository _votableRepository;
    /// <summary>
    /// A controller for creating and managing meme and meme component groupings called topics.
    /// </summary>
    public TopicsController( UserRepository userRepository, TopicRepository topicRepository)
    {
        _userRepository = userRepository;
        _topicRepository = topicRepository;
    }

    /// <summary>
    /// Get all topics
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TopicDTO>>> GetTopics()
    {
        return await _topicRepository.GetTopics();
    }

    /// <summary>
    /// Get a specific topic by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TopicDTO>> GetTopic(string id)
    {
        var topic = await _topicRepository.GetTopic(id);

        if (topic == null) return NotFound();

        return topic.ToTopicDTO();
    }

    /// <summary>
    /// Mod a user for the given topic. Requires the currently logged in user to be the topic owner
    /// </summary>
    [HttpPut]
    [Route("[controller]/{topicId}/mod/{userId}")]
    public async Task<IActionResult> ModUser(string topicId, string userId)
    {
        var topic = await _topicRepository.GetTopic(topicId);
        var user = await _userRepository.GetUser(User.FindFirstValue(ClaimTypes.NameIdentifier));

        if (topic == null) return NotFound("Topic not found");
        if (user == null) return Unauthorized("User not logged in");

        if (topic.Owner != user) return Forbid("Action is forbidden");

        var success = await _topicRepository.ModUser(topic, userId);

        if (!success) return NotFound("User with provided ID not found");

        return Ok();
    }

    /// <summary>
    /// Create a topic with the currently logged in user as the owner
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TopicDTO>> CreateTopic(TopicCreationDTO topicCreationDTO)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();
        var topic = await _topicRepository.CreateTopic(topicCreationDTO, userId);

        return CreatedAtAction("GetTopic", new { id = topic.Id }, topic);
    }

    /// <summary>
    /// Delete Topic
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteTopic(string id)
    {
        var topic = await _topicRepository.GetTopic(id);
        var user = await _userRepository.GetUser(User.FindFirstValue(ClaimTypes.NameIdentifier));
        
        if (topic == null) return NotFound("Topic not found");
        if (user == null) return Unauthorized("User not logged in");

        var success = await _topicRepository.DeleteTopic(topic, user);

        if (!success) return Unauthorized();
        return NoContent();
    }


    /// <summary>
    ///Delete a votable in a topic that you own or moderate
    /// </summary>
    [HttpDelete]
    [Route("[controller]/{id}")]
    public async Task<IActionResult> DeleteVotable(string id)
    {
        var votable = await _votableRepository.GetVotable(id);
        var user = await _userRepository.GetUser(User.FindFirstValue(ClaimTypes.NameIdentifier));

        if (votable == null) return NotFound("Votable not found");
        if (user == null) return Unauthorized("User not logged in");

        var success = await _votableRepository.DeleteVotable(votable, user);
        if (!success) return Forbid("Action is forbidden");
       
        return Ok();
    }
}
