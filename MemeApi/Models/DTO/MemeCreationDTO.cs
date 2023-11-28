using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO;

/// <summary>
/// A DTO with information to create a meme 
/// </summary>
public class MemeCreationDTO
{
    /// <summary>
    /// The visual component of the meme
    /// </summary>
    [Required]
    public IFormFile VisualFile { get; set; }
    //public IFormFile SoundFile { get; set; }
    /// <summary>
    /// The textual top component of the meme
    /// </summary>
    public string Toptext { get; set; }
    /// <summary>
    /// The textual bottom component of the meme
    /// </summary>
    public string Bottomtext { get; set; }
    /// <summary>
    /// Optional name for the visual component
    /// </summary>
    public string FileName { get; set; }
    /// <summary>
    /// The list of topics that the Meme belongs to.
    /// </summary>
    public IEnumerable<string> Topics { get; set; }
}