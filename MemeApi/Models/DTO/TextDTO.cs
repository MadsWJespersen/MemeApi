using MemeApi.Models.Entity;
using System;
using System.Collections.Generic;

namespace MemeApi.Models.DTO;

/// <summary>
/// A DTO for textual meme components
/// </summary>
/// <param name="Id"> Id of the Text </param>
/// <param name="Text"> Actual content of the text </param>
/// <param name="Position"> Position of the textual component </param>
/// <param name="Topics"> Topics that the text belongs to </param>
/// <param name="CreatedAt"> The time at which the Text was created </param>
public record TextDTO(string Id, string Text, MemeTextPosition Position, List<string> Topics, DateTime CreatedAt);
