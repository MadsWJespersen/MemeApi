using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MemeApi.library.Extensions;
using MemeApi.Models.Context;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MemeApi.library.repositories;

public class TopicRepository
{
    private readonly MemeContext _context;
    private UserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public TopicRepository(MemeContext context, UserRepository userRepository, IConfiguration configuration)
    {
        _context = context;
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<List<TopicDTO>> GetTopics()
    {
        var topics = await _context.Topics
            .Include(t => t.Owner)
            .Include(t => t.Moderators)
            .Select(t => t.ToTopicDTO())
            .ToListAsync();

        return topics;
    }

    public async Task<Topic> GetTopic(string id)
    {
        var topic = await _context.Topics.FindAsync(id);
        return topic;
    }

    public async Task<Topic> GetTopicByName(string name)
    {
        var topic = await _context.Topics.Where(t => t.Name == name).FirstOrDefaultAsync();
        return topic;
    }

    public async Task<List<Topic>> GetTopicsByNameForUser(IEnumerable<string> topicNames, string userId = null)
    {
        return await GetTopicsByNameOrDefault(topicNames, userId);
    }

    public async Task<List<Topic>> GetTopicsByNameOrDefault(IEnumerable<string> topicNames, string userId = null)
    {
        if(topicNames != null)
        {
            var topics = await _context.Topics.Where(t => topicNames.Contains(t.Name)).ToListAsync();
            var filteredTopics = topics.Where(t => t.CanUserPost(userId)).ToList();
            if (!filteredTopics.Any()) return new List<Topic> { await _context.Topics.FirstAsync(t => t.Name == _configuration["Topic.Default.Topicname"]) };
            return filteredTopics;
        }

        return new List<Topic> { await _context.Topics.FirstAsync(t => t.Name == _configuration["Topic.Default.Topicname"]) };
    }
    public async Task<Topic> GetDefaultTopic()
    {
        return await _context.Topics.FirstAsync(t => t.Name == _configuration["Topic.Default.Topicname"]);
    }

    public async Task<bool> UpdateTopic(string id, string name = null, string description = null)
    {
        var topic = await _context.Topics.FindAsync(id);

        if (topic == null)
        {
            return false;
        }

        if (name == null && description == null) return false;

        if(name != null)
        {
            topic.Name = name;
        }

        if (description != null)
        {
            topic.Description = description;
        }

        try
        {
            topic.LastUpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TopicExists(id))
            {
                return false;
            }
            else
            {
                throw;
            }
        }

        return true;
    }

    public async Task<TopicDTO> CreateTopic(TopicCreationDTO topicCreationDTO, string userId)
    {
        var user = await _userRepository.GetUser(userId);

        var topic = new Topic()
        {
            Id = Guid.NewGuid().ToString(),
            Owner = user,
            Name = topicCreationDTO.TopicName,
            Description = topicCreationDTO.Description,
            Moderators = new List<User>(),
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };

        _context.Topics.Add(topic);
        await _context.SaveChangesAsync();
        return topic.ToTopicDTO();
    }

    public async Task<bool> DeleteTopic(Topic topic, User user)
    {
        if (topic.Owner != user) return false;

        _context.Topics.Remove(topic);
        await _context.SaveChangesAsync();

        return true;
    }

    private bool TopicExists(string id)
    {
        return _context.Topics.Any(e => e.Id == id);
    }

    internal async Task<bool> ModUser(Topic topic, string userId)
    {
        var user = await _userRepository.GetUser(userId);
        if (user == null) return false;
        topic.Moderators.Add(user); // TODO: handle adding the same person twice
        await _context.SaveChangesAsync();
        return true;
    }
}
