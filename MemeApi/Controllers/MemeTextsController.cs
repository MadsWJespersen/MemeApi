﻿using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MemeApi.library.repositories;

namespace MemeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemeTextsController : ControllerBase
    {
        private readonly TextRepository _textRepository;

        public MemeTextsController( TextRepository textRepository)
        {
            _textRepository = textRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemeText>>> GetBottomTexts() => await _textRepository.GetBottomTexts();

        [HttpGet("{id}")]
        public async Task<ActionResult<MemeText>> GetMemeBottomText(long id)
        {
            var memeBottomText = await _textRepository.GetBottomText(id);

            if (memeBottomText == null)
            {
                return NotFound();
            }

            return Ok(memeBottomText);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMemeText(long id, string newMemeBottomText, MemeTextPosition? newMemeTextPosition = null)
        {
            var memeText = await _textRepository.UpdateText(id, newMemeBottomText, newMemeTextPosition);

            if (!memeText) return NotFound();
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<MemeText>> CreateMemeBottomText(string text, MemeTextPosition position)
        {
            var memeText = await _textRepository.CreateText(text, position);
            return CreatedAtAction("CreateMemeBottomText", new { id = memeText.Id }, memeText);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMemeBottomText(long id)
        {
            var removed = await _textRepository.RemoveText(id);

            if (!removed) return NotFound();
            return NoContent();
        }
    }
}
