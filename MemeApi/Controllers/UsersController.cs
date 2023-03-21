﻿using MemeApi.Models;
using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MemeApi.library;
using MemeApi.library.repositories;
using MemeApi.Models.DTO;
using Microsoft.AspNetCore.Identity;

namespace MemeApi.Controllers
{
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IMailSender _mailSender;
        private readonly IConfiguration _configuration;

        public UsersController(SignInManager<User> signInManager, UserManager<User> userManager, UserRepository userRepository, IMailSender mailSender, IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userRepository = userRepository;
            _mailSender = mailSender;
            _configuration = configuration;
        }

        // GET: api/Users
        [HttpGet]
        [Route("[controller]")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _userRepository.GetUsers();
        }

        // GET: api/Users/5
        [HttpGet]
        [Route("[controller]/{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _userRepository.GetUser(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Route("[controller]/update")]
        public async Task<IActionResult> UpdateUser([FromForm]UserUpdateDTO updateDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var (updated, message) = await _userRepository.UpdateUser(int.Parse(userId), updateDto);
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

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Route("[controller]/register")]
        [AllowAnonymous]
        public async Task<ActionResult<UserInfoDTO>> Register([FromForm]UserCreationDTO userDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.Values);

            var user = new User
            {
                UserName = userDTO.Username, 
                Email = userDTO.Email
            };
            var result = await _userManager.CreateAsync(user, userDTO.Password);
            //todo add better handling of already used email or username
            if (!result.Succeeded)
                return BadRequest(userDTO);

            await _signInManager.SignInAsync(user, false);
            var url = _configuration["Media.Host"] + "profilepic/" + (!string.IsNullOrEmpty(user.ProfilePicFile) ? user.ProfilePicFile : "default.jpg");
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new UserInfoDTO { ProfilePicURl = url});
        }

        [HttpPost]
        [Route("[controller]/recover")]
        public async Task<bool> RecoverUser([FromForm]string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            return false;
            // Currently don't have a sender that can send recovery emails
            // return _mailSender.sendMail(new MailAddress(user.Email, user.UserName), "Recover account", "beans");
        }

        // DELETE: api/Users/5
        [HttpDelete]
        [Route("[controller]/{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            if (await _userRepository.DeleteUser(id)) return NotFound();

            return NoContent();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("[controller]/login")]
        public async Task<ActionResult<UserInfoDTO>> Login([FromForm]UserLoginDTO loginDTO)
        {
            var user = await _userManager.FindByNameAsync(loginDTO.Username);
            if(user == null) return Unauthorized("The entered information was not correct");

            var result = await _signInManager.PasswordSignInAsync(user, loginDTO.Password, false, false);
            if (!result.Succeeded)
                return Unauthorized("The entered information was not correct");
            var url = _configuration["Media.Host"] + "profilepic/" + (!string.IsNullOrEmpty(user.ProfilePicFile) ? user.ProfilePicFile : "default.jpg");
            return Ok(new UserInfoDTO { ProfilePicURl = url });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("[controller]/logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(userId == null) return NotFound();
            await _signInManager.SignOutAsync();
            return Ok();
        }
    }
}
