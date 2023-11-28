using System;
using System.Collections.Generic;

namespace MemeApi.Models.DTO;

/// <summary>
/// A DTO for a topic
/// </summary>
/// <param name="Id"> Topic Id </param>
/// <param name="Name"> Topic name </param>
/// <param name="Description"> Description of the topic </param>
/// <param name="Owner"> Topic owner name </param>
/// <param name="Moderators"> Topic moderator names </param>
/// <param name="CreatedAt"> Topic creation time </param>
/// <param name="UpdatedAt"> Latest update to topic </param>
public record TopicDTO(string Id, string Name, string Description, string Owner, List<string> Moderators, DateTime CreatedAt, DateTime UpdatedAt);
