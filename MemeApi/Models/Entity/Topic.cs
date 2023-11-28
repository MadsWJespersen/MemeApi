using System;
using System.Collections.Generic;

namespace MemeApi.Models.Entity;

public class Topic
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public User Owner { get; set; }
    public string OwnerId { get; set; }
    public List<User> Moderators { get; set; }
    public List<Votable> Votables { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public bool HasRestrictedPosting { get; set; }
}
