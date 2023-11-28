using System;
using System.Collections.Generic;

namespace MemeApi.Models.DTO;

/// <summary>
/// A DTO for memes
/// </summary>
/// <param name="Id"></param>
/// <param name="MemeVisual"> Visual component of the meme </param>
/// <param name="Toptext"> Textual top component of the meme </param>
/// <param name="BottomText"> Textual bottom component of the meme </param>
/// <param name="Topics"> Topics that the meme belongs to </param>
/// <param name="CreatedAt"></param>
public record MemeDTO(string Id, string MemeVisual, string Toptext, string BottomText, List<string> Topics, DateTime CreatedAt);
