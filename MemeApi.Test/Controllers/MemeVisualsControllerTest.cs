using FluentAssertions;
using MemeApi.Controllers;
using MemeApi.library.Extensions;
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
    public class MemeVisualsControllerTest : MemeTestBase
    {
        [Fact]
        public async Task GIVEN_DummyFile_WHEN_CreatingMemeVisual_THEN_MemeVisualIsCreatedWithProperValues()
        {
            var controller = new VisualsController(_visualRepository, _settings);
            controller.ControllerContext.HttpContext = GetMockedHttpContext();

            // given
            var fileStream = new MemoryStream(5);
            var file = new FormFile(fileStream, 0, 5, "filestream", "test");

            // When
            var createTask = controller.PostMemeVisual(file);

            // Then
            var createdMemeVisual = await ActionResultUtils.ActionResultToValueAndAssertCreated(createTask);

            (await _context.Visuals.CountAsync()).Should().Be(1);
            createdMemeVisual.Filename.Should().Be(file.FileName);
        }

        [Fact]
        public async Task GIVEN_CreatedDummyMemeBottomText_WHEN_GettingMemeBottomText_THEN_MemeBottomTextHasProperValues()
        {
            var controller = new VisualsController(_visualRepository, _settings);

            // given
            var visual = new MemeVisual()
            {
                Id = Guid.NewGuid().ToString(),
                Filename = "Test"
            };
            _context.Visuals.Add(visual);
            await _context.SaveChangesAsync();

            // When
            var expected = visual.ToRandomComponentDTO("");
            var result = (await controller.GetMemeVisual(visual.Id)).Result;

            // Then

            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var foundMemeText = ((OkObjectResult)result).Value as RandomComponentDTO;

            foundMemeText.data.Should().Be(expected.data);
            foundMemeText.id.Should().Be(expected.id);
            foundMemeText.votes.Should().Be(expected.votes);
        }

        [Fact]
        public async Task GIVEN_TwoDummyFilesWithSameName_WHEN_CreatingMemeVisuals_THEN_SecondMemeVisualIsHasDifferentName()
        {
            var controller = new VisualsController(_visualRepository, _settings);
            controller.ControllerContext.HttpContext = GetMockedHttpContext();

            // given
            var fileStream = new MemoryStream(5);
            var file = new FormFile(fileStream, 0, 5, "filestream", "test");
            var file2 = new FormFile(fileStream, 0, 5, "filestream", "test");

            // When
            var createTask = controller.PostMemeVisual(file);
            var createTask2 = controller.PostMemeVisual(file2);

            // Then
            var createdMemeVisual = await ActionResultUtils.ActionResultToValueAndAssertCreated(createTask);
            var createdMemeVisual2 = await ActionResultUtils.ActionResultToValueAndAssertCreated(createTask2);

            (await _context.Visuals.CountAsync()).Should().Be(2);
            createdMemeVisual.Filename.Should().NotBe(createdMemeVisual2.Filename);
            createdMemeVisual2.Filename.Should().NotBe(file2.FileName);
        }

        [Fact]
        public async Task GIVEN_LargeDummyFile_WHEN_CreatingMemeVisual_THEN_MemeVisualIsNotCreated()
        {
            var controller = new VisualsController(_visualRepository, _settings);

            // given
            var fileStream = new MemoryStream(5000001);
            var file = new FormFile(fileStream, 0, 50000001, "filestream", "test");

            // When
            var createResult = (await controller.PostMemeVisual(file)).Result;

            // Then
            createResult.Should().NotBeNull();
            createResult.Should().BeOfType<StatusCodeResult>();
            ((StatusCodeResult)createResult).StatusCode.Should().Be(413);

            (await _context.Visuals.CountAsync()).Should().Be(0);
        }



        [Fact]
        public async Task GIVEN_CreatedDummyMemeBottomText_WHEN_Deleting_THEN_MemeVisualIsDeleted()
        {
            var controller = new VisualsController(_visualRepository, _settings);

            // given
            var memeVisual = new MemeVisual()
            {
                Id = Guid.NewGuid().ToString(),
                Filename = "Test"
            };
            _context.Visuals.Add(memeVisual);


            // When
            var result = await controller.DeleteMemeVisual(memeVisual.Id);

            // Then
            result.Should().NotBeNull();
            result.Should().BeOfType<NoContentResult>();
            (await _context.Users.CountAsync()).Should().Be(0);
        }
    }
}
