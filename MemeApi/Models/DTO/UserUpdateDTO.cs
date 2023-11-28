using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MemeApi.Models.DTO;

/// <summary>
/// A DTO for Updating user information
/// </summary>
public record UserUpdateDTO
{
    /// <summary>
    /// New username
    /// </summary>
    [Required]
    public string NewUsername { get; init; }

    /// <summary>
    /// New email
    /// </summary>
    public string NewEmail { get; init; }

    /// <summary>
    /// Current password
    /// </summary>
    public string CurrentPassword { get; init; }

    /// <summary>
    /// New password
    /// </summary>
    public string NewPassword { get; init; }

    /// <summary>
    /// New profile picture
    /// </summary>
    public IFormFile NewProfilePic { get; init; }
}