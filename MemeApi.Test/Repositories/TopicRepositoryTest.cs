using FluentAssertions;
using MemeApi.Models.Entity;
using MemeApi.Test.library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MemeApi.Test.Repositories
{
    public class TopicRepositoryTest : MemeTestBase
    {
        [Fact]
        public async Task GIVEN_Votable_WHEN_OwnerOfTopicDeleting_THEN_VotableDeleted()
        {
            // given
            var user = new User() { Id = Guid.NewGuid().ToString() };
            var topic = new Topic { Id = Guid.NewGuid().ToString(), Name = "Test", Owner = user};
            var votable = new Votable {Id = Guid.NewGuid().ToString(), Topics = new List<Topic>() { topic } };
            _context.Votables.Add(votable);
            _context.SaveChanges();

            // When
            var result = await _votableRepository.DeleteVotable(votable, user);

            // Then
            result.Should().BeTrue();
            _context.Votables.Count().Should().Be(0);
        }

        [Fact]
        public async Task GIVEN_TopicWithRestrictedPosting_WHEN_NotAllowedUserPosts_THEN_PostDoesNotHaveTopic()
        {
            // given
            var user = new User() { Id = Guid.NewGuid().ToString() };
            var topic = new Topic { Id = Guid.NewGuid().ToString(), Name = "Testawda", HasRestrictedPosting = true, Moderators = new List<User> {} };
            var text = "";

            _context.Users.Add(user);
            _context.Topics.Add(topic);
            _context.SaveChanges();
            // When
            var result = await _textRepository.CreateText(text, MemeTextPosition.TopText, new List<string> { topic.Name }, user.Id);

            // Then
            result.Topics.Should().NotContain(topic);
            result.Topics.Count().Should().Be(1);
        }

        [Fact]
        public async Task GIVEN_TopicWithRestrictedPosting_WHEN_AllowedUserPosts_THEN_PostDoesHaveTopic()
        {
            // given
            var user = new User() { Id = Guid.NewGuid().ToString() };
            var topic = new Topic { Id = Guid.NewGuid().ToString(), Name = "Testawda", HasRestrictedPosting = true, Moderators = new List<User> { user } };
            var text = "";

            _context.Users.Add(user);
            _context.Topics.Add(topic);
            _context.SaveChanges();
            // When
            var result = await _textRepository.CreateText(text, MemeTextPosition.TopText, new List<string> { topic.Name }, user.Id);

            // Then
            result.Topics.Should().Contain(topic);
            result.Topics.Count().Should().Be(1);
        }

        [Fact]
        public async Task GIVEN_DefaultTopic_WHEN_AnonymousPosts_THEN_PostDoesHaveDefaultTopic()
        {
            var defaultTopic = await _topicRepository.GetDefaultTopic();

            // When
            var result = await _textRepository.CreateText("test", MemeTextPosition.TopText);

            // Then
            result.Topics.Should().Contain(defaultTopic);
            result.Topics.Count().Should().Be(1);
        }

        [Fact]
        public async Task GIVEN_DefaultTopic_WHEN_UserPosts_THEN_PostDoesHaveDefaultTopic()
        {
            var defaultTopic = await _topicRepository.GetDefaultTopic();
            var user = new User() { Id = Guid.NewGuid().ToString() };

            _context.Users.Add(user);
            _context.SaveChanges();
            // When
            var result = await _textRepository.CreateText("test", MemeTextPosition.TopText, userId: user.Id);

            // Then
            result.Topics.Should().Contain(defaultTopic);
            result.Topics.Count().Should().Be(1);
        }


        [Fact]
        public async Task GIVEN_Votable_WHEN_ModeratorOfTopicDeleting_THEN_VotableDeleted()
        {
            // given
            var user = new User() { Id = Guid.NewGuid().ToString() };
            var topic = new Topic { Id = Guid.NewGuid().ToString(), Name = "Test", Moderators = new List<User> { user } };
            var votable = new Votable {
                Id = Guid.NewGuid().ToString(),
                Topics = new List<Topic>() { topic }
            };
            _context.Votables.Add(votable);
            _context.SaveChanges();

            // When
            var result = await _votableRepository.DeleteVotable(votable, user);

            // Then
            result.Should().BeTrue();
            _context.Votables.Count().Should().Be(0);
        }

        [Fact]
        public async Task GIVEN_Votable_WHEN_NonModeratorOrOwnerOfTopicDeleting_THEN_VotableNotDeleted()
        {
            // given
            var user = new User() { Id = Guid.NewGuid().ToString() };
            var topic = new Topic { Id = Guid.NewGuid().ToString(), Name = "Test" , Owner = new User() { Id = Guid.NewGuid().ToString() }, Moderators = new List<User>()};
            var votable = new Votable {Id = Guid.NewGuid().ToString(), Topics = new List<Topic>() { topic }};
            _context.Votables.Add(votable);
            _context.SaveChanges();
            // When
            var result = await _votableRepository.DeleteVotable(votable, user);

            // Then
            result.Should().BeFalse();
            _context.Votables.Count().Should().Be(1);
        }

        [Fact]
        public async Task GIVEN_VotableInMultipleTopics_WHEN_OwnerDeleting_THEN_VotableNotDeletedButRemovedFromTopic()
        {
            // given
            var user = new User() { Id = Guid.NewGuid().ToString() };
            var topic = new Topic { Id = Guid.NewGuid().ToString(), Name = "Test", Owner = user };
            var topic2 = new Topic { Id = Guid.NewGuid().ToString(), Name = "Test2", Owner = new User() { Id = Guid.NewGuid().ToString() }, Moderators = new List<User>() };
            var votable = new Votable { Id = Guid.NewGuid().ToString(), Topics = new List<Topic>() { topic, topic2 } };
            _context.Votables.Add(votable);
            _context.SaveChanges();

            // When
            var result = await _votableRepository.DeleteVotable(votable, user);
            var resultVotable = await _votableRepository.GetVotable(votable.Id);
            // Then
            result.Should().BeTrue();
            resultVotable.Topics.Count().Should().Be(1);
            resultVotable.Topics.Should().NotContain(topic);
        }

        [Fact]
        public async Task GIVEN_VotableInMultipleTopics_WHEN_ModeratorDeleting_THEN_VotableNotDeletedButRemovedFromTopic()
        {
            // given
            var user = new User() { Id = Guid.NewGuid().ToString() };
            var topic = new Topic { Id = Guid.NewGuid().ToString(), Name = "Test", Moderators = new List<User> { user } };
            var topic2 = new Topic { Id = Guid.NewGuid().ToString(), Name = "Test2", Owner = new User() { Id = Guid.NewGuid().ToString() }, Moderators = new List<User>() };
            var votable = new Votable { Id = Guid.NewGuid().ToString(), Topics = new List<Topic>() { topic, topic2 } };
            _context.Votables.Add(votable);
            _context.SaveChanges();

            // When
            var result = await _votableRepository.DeleteVotable(votable, user);
            var resultVotable = await _votableRepository.GetVotable(votable.Id);
            // Then
            result.Should().BeTrue();
            resultVotable.Topics.Count().Should().Be(1);
            resultVotable.Topics.Should().NotContain(topic);
        }
    }
}
