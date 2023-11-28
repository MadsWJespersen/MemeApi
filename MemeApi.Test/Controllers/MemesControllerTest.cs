using FluentAssertions;
using MemeApi.Controllers;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using MemeApi.Test.library;
using MemeApi.Test.utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace MemeApi.Test.Controllers
{
    public class MemesControllerTest : MemeTestBase
    {
        [Fact]
        public async Task GIVEN_Visual_WHEN_CreatingMeme_THEN_MemeIsCreated()
        {
            var controller = new MemesController(_memeRepository, _memeRenderingService);

            // given
            var filename = "test.png";

            var memeCreationDTO = new MemeCreationDTO()
            {
                VisualFile = CreateFormFile(5, filename),
            };

            // When
            var creationTask = controller.PostMeme(memeCreationDTO);

            // Then
            var createdMeme = await ActionResultUtils.ActionResultToValueAndAssertCreated(creationTask);

            (await _context.Memes.CountAsync()).Should().Be(1);
            (await _context.Visuals.CountAsync()).Should().Be(1);

            createdMeme.MemeVisual.Should().Be(filename);
        }

        [Fact]
        public async Task GIVEN_CreatedDummyMemeBottomText_WHEN_GettingMemeBottomText_THEN_MemeBottomTextHasProperValues()
        {
            var controller = new MemesController(_memeRepository, _memeRenderingService);

            // given
            var visual = new MemeVisual()
            {
                Id = Guid.NewGuid().ToString(),
                Filename = "Test"
            };
            var meme = new Meme
            {
                Id = Guid.NewGuid().ToString(),
                MemeVisual = visual,
            };
            _context.Memes.Add(meme);
            await _context.SaveChangesAsync();

            // When
            var response = await controller.GetMeme(meme.Id);
            var result = response.Result;

            // Then

            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var foundMemeText = ((OkObjectResult)result).Value as MemeDTO;

            foundMemeText.MemeVisual.Should().Be(visual.Filename);
            foundMemeText.Id.Should().Be(meme.Id);
        }

        public static IFormFile CreateFormFile(int size, string filename)
        {
            var fileStream = new MemoryStream(size);
            var file = new FormFile(fileStream, 0, size, "filestream", filename);
            return file;
        }
    }
}
