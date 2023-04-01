﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MemeApi.library.repositories
{
    public class TextRepository
    {
        private readonly MemeContext _context;
        private readonly TopicRepository _topicRepository;

        public TextRepository(MemeContext context, TopicRepository topicRepository)
        {
            _context = context;
            _topicRepository = topicRepository;
        }

        public async Task<List<MemeText>> GetTexts(MemeTextPosition? type = null)
        {
            var texts = _context.Texts.Include(x => x.Votes).Include(t => t.Topics);
            if (type != null)
            {
                return await texts.Where(x => x.Position == type).ToListAsync();
            }
            
            return await texts.ToListAsync();
        }

        public async Task<MemeText> GetText(string id)
        {
            return await _context.Texts.Include(x => x.Votes).Include(t => t.Topics).FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<bool> UpdateText(string id, string newMemeBottomText, MemeTextPosition? newMemeTextPosition = null)
        {
            var memeText = await _context.Texts.FindAsync(id);

            if (memeText == null)
            {
                return false;
            }
            memeText.Text = newMemeBottomText;
            if (newMemeTextPosition != null)
            {
                memeText.Position = (MemeTextPosition)newMemeTextPosition;
            }

            try
            {
                memeText.LastUpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemeBottomTextExists(id))
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

        public async Task<MemeText> CreateText(string text, MemeTextPosition position, IEnumerable<string> topicNames = null)
        {
            var topics = await _topicRepository.GetTopicsByNameOrDefault(topicNames);
            var memeText = new MemeText
            {
                Id = Guid.NewGuid().ToString(),
                Text = text,
                Position = position,
                Topics = topics,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
            };

            _context.Texts.Add(memeText);
            await _context.SaveChangesAsync();
            return memeText;
        }

        public async Task<bool> RemoveText(string id)
        {
            var memeBottomText = await _context.Texts.FindAsync(id);
            if (memeBottomText == null) return false;
            

            _context.Texts.Remove(memeBottomText);
            await _context.SaveChangesAsync();

            return true;
        }

        private bool MemeBottomTextExists(string id)
        {
            return _context.Texts.Any(e => e.Id == id);
        }
    }
}
