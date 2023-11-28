using System.Collections.Generic;

namespace MemeApi.Models.DTO;


/// <summary>
/// A DTO of user info
/// </summary>
/// <param name="UserName"> Users name </param>
/// <param name="ProfilePicURl"> Users profile picture URL </param>
/// <param name="Topics"> Topics that the users owns </param>
public record UserInfoDTO(string UserName, string ProfilePicURl, List<string> Topics);
