﻿using System;
using MemeApi.library.repositories;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using MemeApi.library.Extensions;
using Microsoft.AspNetCore.Authorization;
namespace MemeApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MemesController : ControllerBase
    {
        private readonly MemeRepository _memeRepository;
        public MemesController(MemeRepository memeRepository)
        {
            _memeRepository = memeRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Meme>>> GetMemes()
        {
            return await _memeRepository.GetMemes();
        }

        
        [HttpGet("{id}")]
        public async Task<ActionResult<Meme>> GetMeme(int id)
        {
            var meme = await _memeRepository.GetMeme(id);

            if (meme == null)
            {
                return NotFound();
            }

            return meme;
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMeme(int id, Meme meme)
        {
            if (id != meme.Id)
            {
                return BadRequest();
            }

            if (await _memeRepository.ModifyMeme(id, meme)) return NotFound();

            return NoContent();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<Meme>> PostMeme([FromForm]MemeCreationDTO memeCreationDto)
        {
            var meme = await _memeRepository.CreateMeme(memeCreationDto);
            return CreatedAtAction(nameof(GetMeme), new { id = meme.Id }, meme);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeme(int id)
        {
            if (await _memeRepository.DeleteMeme(id)) return NotFound();

            return NoContent();
        }

        [HttpGet]
        [Route("random")]
        public async Task<ActionResult<Meme>> RandomMeme()
        {
            return Ok(this.RandomItem(await _memeRepository.GetMemes()));
        }
    }
}

