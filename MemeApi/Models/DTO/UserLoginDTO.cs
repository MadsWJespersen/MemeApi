using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO;


/// <summary>
/// A DTO with information for logging into a user
/// </summary>
/// <param name="Username"> Username or email of the user </param>
/// <param name="Password"> Password of the user </param>
public record UserLoginDTO([property: Required] string Username, [property: Required] string Password);
