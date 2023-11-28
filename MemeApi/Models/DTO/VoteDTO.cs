using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO;




/// <param name="ElementIDs"></param>
/// <param name="UpVote"></param>///<Summary>
/// A DTO for Votes
///</Summary>
public record VoteDTO([property: Required] List<string> ElementIDs, [property: Required] Upvote UpVote);
