using System;
using System.Collections.Generic;

namespace MemeApi.Models.DTO;

/// <param name="Id"> Filename of the Visual </param>
/// <param name="Filename"> Filename of the Visual </param>
/// <param name="Topics"> Topics that the Visual are in </param>
/// <param name="CreatedAt"> The time at which the Visual was created </param>
public record VisualDTO(string Id, string Filename, List<string> Topics, DateTime CreatedAt);
