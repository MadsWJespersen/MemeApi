using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using MemeApi.Controllers;
using MemeApi.library.Extensions;
using MemeApi.library.repositories;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using MemeApi.Test.library;
using MemeApi.Test.utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MemeApi.Test.Controllers
{
    public class MemeTextsControllerTest : MemeTestBase
    {
        [Theory]
        [InlineData(MemeTextPosition.TopText)]
        [InlineData(MemeTextPosition.BottomText)]
        public async Task GIVEN_DummyMemeText_WHEN_CreatingMemeBottomText_THEN_MemeBottomTextIsCreatedWithProperValues(MemeTextPosition memePosition)
        {
            var controller = new TextsController(_textRepository);
            controller.ControllerContext.HttpContext = GetMockedHttpContext();

            // given
            var memeText = new TextCreationDTO()
            {
                Text = "Test",
                position = memePosition
            }; 

            // When
            var createTask = controller.CreateMemeText(memeText);

            // Then
            var createdMemeText = await ActionResultUtils.ActionResultToValueAndAssertCreated(createTask);
                
            (await _context.Texts.CountAsync()).Should().Be(1);
            createdMemeText.Text.Should().Be(memeText.Text);
        }

        [Fact]
        public async Task GIVEN_CreatedDummyMemeBottomText_WHEN_GettingMemeBottomText_THEN_MemeBottomTextHasProperValues()
        {
            var controller = new TextsController(_textRepository);
            controller.ControllerContext.HttpContext = GetMockedHttpContext();

            // given
            var memeText = new MemeText()
            {
                Id = Guid.NewGuid().ToString(),
                Text = "Test",
                Position = MemeTextPosition.BottomText,
                Topics = new List<Topic>() { await _topicRepository.GetDefaultTopic() }
            };
            _context.Texts.Add(memeText);
            await _context.SaveChangesAsync();

            // When
            var expected = memeText.ToTextDTO();
            var result = (await controller.GetMemeText(memeText.Id)).Result;

            // Then

            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var foundMemeText = ((OkObjectResult)result).Value as TextDTO;

            foundMemeText.Text.Should().Be(expected.Text);
            foundMemeText.Id.Should().Be(expected.Id);
            foundMemeText.Position.Should().Be(expected.Position);
        }

        //[Theory]
        //[InlineData(MemeTextPosition.TopText, MemeTextPosition.BottomText)]
        //[InlineData(MemeTextPosition.BottomText, MemeTextPosition.TopText)]
        //public async Task GIVEN_CreatedDummyMemeBottomText_WHEN_Updating_THEN_MemeBottomTextIsUpdatedWithGivenValues(MemeTextPosition memePosition, MemeTextPosition newMemePosition)
        //{
        //    var controller = new TextsController(_textRepository, _mapper);

        //    // given
        //    var newMemeText = "Test2";

        //    var memeText = new MemeText()
        //    {
        //        Text = "Test",
        //        Position = memePosition
        //    };
        //    _context.Texts.Add(memeText);

        //    // When
        //    await controller.UpdateMemeText(memeText.Id, newMemeText, newMemePosition);

        //    // Then
        //    var foundMemeBottomText = await
        //        ActionResultUtils.ActionResultToValueAndAssertOk(
        //            controller.GetMemeText(memeText.Id));

        //    foundMemeBottomText.Text.Should().Be(newMemeText);
        //}

        [Fact]
        public async Task GIVEN_CreatedDummyMemeBottomText_WHEN_Deleting_THEN_MemeBottomTextIsDeleted()
        {
            var controller = new TextsController(_textRepository);
            controller.ControllerContext.HttpContext = GetMockedHttpContext();

            // given

            var memeText = new MemeText()
            {
                Id = Guid.NewGuid().ToString(),
                Text = "Test",
                Position = MemeTextPosition.BottomText
            };
            _context.Texts.Add(memeText);

            // When
            var result = await controller.DeleteMemeText(memeText.Id);

            // Then

            result.Should().NotBeNull();
            result.Should().BeOfType<NoContentResult>();
            (await _context.Texts.CountAsync()).Should().Be(0);
        }
    }
}
