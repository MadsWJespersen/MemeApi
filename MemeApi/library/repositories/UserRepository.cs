using MemeApi.library.Services.Files;
using MemeApi.Models.Context;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MemeApi.library.repositories;

public class UserRepository
{
    private readonly MemeContext _memeContext;
    private readonly UserManager<User> _userManager;
    private readonly IFileSaver _fileSaver;
    private static readonly Random _random = Random.Shared;

    public UserRepository(MemeContext memeContext, UserManager<User> userManager, IFileSaver fileSaver)
    {
        _memeContext = memeContext;
        _userManager = userManager;
        _fileSaver = fileSaver;
    }

    public async Task<List<User>> GetUsers()
    {
        return await _memeContext.Users.Include(u => u.Topics).ToListAsync();
    }

    public async Task<User> GetUser(string id)
    {
        return await _memeContext.Users.Include(u => u.Topics).FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User> FindByEmail(string userEmail)
    {
        return await _memeContext.Users.FirstOrDefaultAsync(user => 
            string.Equals(user.Email, userEmail, StringComparison.OrdinalIgnoreCase)
        );
    }

    public async Task UserLoggedIn(User user)
    {
        user.LastLoginAt = DateTime.UtcNow;
        await _memeContext.SaveChangesAsync();
    }

    public async Task<(bool, Errors)> UpdateUser(string id, UserUpdateDTO updateDto)
    {
        var user = await GetUser(id);

        if (user == null)
            return (false,Errors.Not_Found);
        

        user.UserName = updateDto.NewUsername;
        user.Email = updateDto.NewEmail ?? user.Email;

        if (updateDto.NewProfilePic != null ||updateDto.NewEmail != null)
        {

            if (updateDto.CurrentPassword == null) return (false, Errors.Bad_Request);
            var passwordValidator = new PasswordValidator<User>();
            var result = await passwordValidator.ValidateAsync(_userManager, user, updateDto.CurrentPassword);

            if (!result.Succeeded) return (false, Errors.Incorrect_Login);

            if (updateDto.NewProfilePic != null)
            {

                user.ProfilePicFile = updateDto.NewProfilePic.FileName;

                if (_memeContext.Users.Any(x => x.ProfilePicFile == user.ProfilePicFile))
                {
                    user.ProfilePicFile = VisualRepository.RandomString(5) + user.ProfilePicFile;
                }
                await _fileSaver.SaveFile(updateDto.NewProfilePic, "profilepic/", user.ProfilePicFile);
            }

            if (updateDto.NewEmail != null) user.Email = updateDto.NewEmail;
        }

        if (updateDto.NewPassword != null)
        {
            var result =
                await _userManager.ChangePasswordAsync(user, updateDto.CurrentPassword, updateDto.NewPassword);
            if (!result.Succeeded)
            {
                return (false, Errors.Incorrect_Login);
            }
        }

        try
        {
            user.LastUpdatedAt = DateTime.UtcNow;
            await _memeContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserExists(id))
            {
                return (false, Errors.Failure);
            }
        }

        return (true,Errors.NoFailure);
    }

    public async Task<bool> DeleteUser(string id)
    {
        var user = await GetUser(id);
        if (user == null)
        {
            return true;
        }

        _memeContext.Users.Remove(user);
        await _memeContext.SaveChangesAsync();
        return false;
    }

    private bool UserExists(string id)
    {
        return _memeContext.Users.Any(e => e.Id == id);
    }

    public async Task<string> CreateNewPassword(User user)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var password = RandomString(20);
        var result = await _userManager.ResetPasswordAsync(user, token, password);
        int attempts = 0;

        while (!result.Succeeded)
        {
            password = RandomString(20);
            if (attempts > 10) throw new Exception("Couldn't create new password");
            await _userManager.ResetPasswordAsync(user, token, password);
            attempts++;
        }
        await _memeContext.SaveChangesAsync();

        return password;
    }

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
    }
}
