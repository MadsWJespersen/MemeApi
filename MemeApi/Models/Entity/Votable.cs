using System;
using System.Collections.Generic;

namespace MemeApi.Models.Entity;

public class Votable
{
    public string Id { get; set; }
    public List<Topic> Topics { get; set; }
    public List<Vote> Votes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}
