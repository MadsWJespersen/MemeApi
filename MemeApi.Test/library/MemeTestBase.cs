using MemeApi.library.repositories;
using MemeApi.library.Services.Files;
using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using MemeApi.Test.utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MemeApi.library;

namespace MemeApi.Test.library
{
    public class MemeTestBase
    {
        protected readonly MemeContext _context;
        protected readonly VisualRepository _visualRepository;
        protected readonly TextRepository _textRepository;
        protected readonly MemeRepository _memeRepository;
        protected readonly TopicRepository _topicRepository;
        protected readonly UserRepository _userRepository;
        protected readonly VotableRepository _votableRepository;
        protected readonly IConfiguration _configuration;
        protected readonly MemeApiSettings _settings;

        protected readonly MemeRenderingService _memeRenderingService;
        public MemeTestBase()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                {"Topic.Default.Topicname", "test"},
            };

            _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();
            _settings = new MemeApiSettings(_configuration);

            _memeRenderingService = new MemeRenderingService(_settings);
            _context = ContextUtils.CreateMemeTestContext();
            _userRepository = new UserRepository(_context, TestUserManager<User>(), new FileSaverStub());
            _topicRepository = new TopicRepository(_context, _userRepository, _settings);
            _visualRepository = new VisualRepository(_context, new FileSaverStub(), new FileRemoverStub(), _topicRepository);
            _textRepository = new TextRepository(_context, _topicRepository);
            _memeRepository = new MemeRepository(_context, _visualRepository, _textRepository, _topicRepository, _settings);
           _votableRepository = new VotableRepository(_context);

            var defaultTopic = new Topic()
            {
                Id = Guid.NewGuid().ToString(),
                Owner = null,
                Name = _configuration["Topic.Default.Topicname"],
                Description = String.Empty,
                Moderators = new List<User>(),
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            _context.Topics.Add(defaultTopic);
            _context.SaveChanges();

            _configuration = new ConfigurationBuilder()
                .Build();
        }

        public static HttpContext GetMockedHttpContext()
        {
            var context = new DefaultHttpContext();
            var mockSession = new Mock<ISession>();
            context.RequestServices = new Mock<IServiceProvider>().Object;
            context.Session = mockSession.Object;
            return context;
        }
        public static UserManager<TUser> TestUserManager<TUser>(IUserStore<TUser> store = null) where TUser : class
        {
            store = store ?? new Mock<IUserStore<TUser>>().Object;
            var options = new Mock<IOptions<IdentityOptions>>();
            var idOptions = new IdentityOptions();
            idOptions.Lockout.AllowedForNewUsers = false;
            options.Setup(o => o.Value).Returns(idOptions);
            var userValidators = new List<IUserValidator<TUser>>();
            var validator = new Mock<IUserValidator<TUser>>();
            userValidators.Add(validator.Object);
            var pwdValidators = new List<PasswordValidator<TUser>>();
            pwdValidators.Add(new PasswordValidator<TUser>());
            var userManager = new UserManager<TUser>(store, options.Object, new PasswordHasher<TUser>(),
                userValidators, pwdValidators, new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(), null,
                new Mock<ILogger<UserManager<TUser>>>().Object);
            validator.Setup(v => v.ValidateAsync(userManager, It.IsAny<TUser>()))
                .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();
            return userManager;
        }
    }
}
