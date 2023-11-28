using MemeApi.Models.Context;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MemeApi.library.repositories;

public class VotableRepository
{
    private readonly MemeContext _context;

    public VotableRepository(MemeContext context)
    {
        _context = context;
    }

    public async Task<List<Vote>> GetVotes()
    {
        return await _context.Votes.ToListAsync();
    }

    public async Task<Vote> GetVote(string id)
    {
        var vote = await _context.Votes.FindAsync(id);
        return vote;
    }

    public async Task<List<Votable>> FindMany(IEnumerable<string> ids)
    {
        return await _context.Votables
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();
    }


    public Vote? FindByElementAndUser(Votable element, string userId)
    {
        return _context.Votes
            .Select(x => x)
            .Include(x => x.Element)
            .Include(x => x.User)
            .SingleOrDefault(x => x.Element.Id == element.Id && x.User.Id == userId);
    }

    public async Task<Vote> CreateVote(Vote vote)
    {
        _context.Votes.Add(vote);
        await _context.SaveChangesAsync();
        return vote;
    }

    public async Task<bool> DeleteVote(string id)
    {
        var vote = await GetVote(id);
        if (vote == null)
        {
            return false;
        }

        _context.Votes.Remove(vote);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<Votable> GetVotable(string id)
    {
        return await _context.Votables.FindAsync(id);
    }

    public async Task<Vote> ChangeVote(Vote vote, Upvote upvote)
    {
        vote.Upvote = upvote == Upvote.Upvote;
        vote.LastUpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return vote;
    }

    public async Task<bool> DeleteVotable(Votable votable, User user)
    {

        var topicUserModerates = votable.Topics
            .Where(t => t.Owner == user || t.Moderators.Exists(m => m == user))
            .FirstOrDefault();
        if (topicUserModerates == null)
        {
            return false;
        }
        votable.Topics.Remove(topicUserModerates);
        if(votable.Topics.Count == 0)
        {
            _context.Votables.Remove(votable);
        }
        await _context.SaveChangesAsync();

        return true;
    }
}
