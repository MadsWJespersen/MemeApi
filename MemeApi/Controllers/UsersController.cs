using MemeApi.library;
using MemeApi.library.Extensions;
using MemeApi.library.repositories;
using MemeApi.library.Services;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MemeApi.Controllers;

/// <summary>
/// A controller for creating and managing users and their login sessions.
/// </summary>
[ApiController]
public class UsersController : ControllerBase
{
    private readonly UserRepository _userRepository;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IMailSender _mailSender;
    private readonly MemeApiSettings _settings;
    /// <summary>
    /// A controller for creating and managing users and their login sessions.
    /// </summary>
    public UsersController(SignInManager<User> signInManager, UserManager<User> userManager, UserRepository userRepository, IMailSender mailSender, MemeApiSettings settings)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _userRepository = userRepository;
        _mailSender = mailSender;
        _settings = settings;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    [Route("[controller]")]
    public async Task<ActionResult<IEnumerable<UserInfoDTO>>> GetUsers()
    {
        var users = await _userRepository.GetUsers();
        return users.Select(u => u.ToUserInfo(_settings.GetMediaHost())).ToList();
    }

    /// <summary>
    /// Get a specific user by ID
    /// </summary>
    [HttpGet]
    [Route("[controller]/{id}")]
    public async Task<ActionResult<UserInfoDTO>> GetUser(string id)
    {
        var user = await _userRepository.GetUser(id);

        if (user == null)
            return NotFound();

        return Ok(user.ToUserInfo(_settings.GetMediaHost()));
    }

    /// <summary>
    /// Update a user with a new password or profile picture.
    /// </summary>
    [HttpPost]
    [Route("[controller]/update")]
    public async Task<IActionResult> UpdateUser([FromForm]UserUpdateDTO updateDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var (updated, message) = await _userRepository.UpdateUser(userId, updateDto);
        if (!updated)
        {
            return message switch
            {
                Errors.Bad_Request => BadRequest(),
                Errors.Failure => StatusCode(500),
                Errors.Incorrect_Login => Unauthorized(),
                Errors.Not_Found => NotFound(),
                _ => StatusCode(500) // Should never happen
            };
        }
        return NoContent();
    }

    /// <summary>
    /// Register a user with a username, email, and password
    /// </summary>
    [HttpPost]
    [Route("[controller]/register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserInfoDTO>> Register([FromForm]UserCreationDTO userDTO)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.Values);

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = userDTO.Username, 
            Email = userDTO.Email,
            ProfilePicFile = "default.jpg",
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
            Topics = new List<Topic> { }
        };

        var result = await _userManager.CreateAsync(user, userDTO.Password);
        if (!result.Succeeded)
            return Conflict("Email or username already exists");

        await _signInManager.SignInAsync(user, false);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user.ToUserInfo(_settings.GetMediaHost()));
    }

    /// <summary>
    /// intiate the recovery of a user
    /// </summary>
    [HttpPost]
    [Route("[controller]/recover")]
    public async Task<bool> RecoverUser([FromForm]string userEmail)
    {
        var user = await _userRepository.FindByEmail(userEmail);
        if (user == null) return false;
        try
        {
            var password = await _userRepository.CreateNewPassword(user);
            var body = "Hello gamer, you requested a new password, so here it is: \n" + password;
            return _mailSender.sendNoReplyMail(new MailAddress(user.Email, user.UserName), "Recovery password", body);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Delete the currently logged in user
    /// </summary>
    [HttpDelete]
    [Route("[controller]/{id}")]
    public async Task<ActionResult> DeleteUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userRepository.GetUser(userId);
        if (user == null) return NotFound();
        await _userManager.DeleteAsync(user);

        return NoContent();
    }

    /// <summary>
    /// Login to a user with the username and password
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [Route("[controller]/login")]
    public async Task<ActionResult<UserInfoDTO>> Login([FromForm]UserLoginDTO loginDTO)
    {
        var user = await _userManager.FindByNameAsync(loginDTO.Username);
        if (user == null)
            user = await _userRepository.FindByEmail(loginDTO.Username);
            if( user == null) return Unauthorized("The entered information was not correct");

        var result = await _signInManager.PasswordSignInAsync(user, loginDTO.Password, false, false);
        if (!result.Succeeded)
            return Unauthorized("The entered information was not correct");
        await _userRepository.UserLoggedIn(user);
        return Ok(user.ToUserInfo(_settings.GetMediaHost()));
    }

    /// <summary>
    /// Log out of the currently logged in user
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [Route("[controller]/logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userId == null) return Unauthorized();
        await _signInManager.SignOutAsync();
        return Ok();
    }
}
