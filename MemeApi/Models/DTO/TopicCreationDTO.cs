using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO;

/// <summary>
/// A DTO for creating Topics
/// </summary>
public record TopicCreationDTO
{
    /// <summary>
    /// Name of the topic
    /// </summary>
    [Required]
    public string TopicName { get; init; }

    /// <summary>
    /// Description of the topic
    /// </summary>
    [Required]
    public string Description { get; init; }
}
