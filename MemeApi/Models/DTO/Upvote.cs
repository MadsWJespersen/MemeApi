namespace MemeApi.Models.DTO;

/// <summary>
/// Represents the status of a vote
/// </summary>
public enum Upvote
{
    /// <summary>
    /// User likes this and upvotes it
    /// </summary>
    Upvote,
    /// <summary>
    /// User dislikes this and upvotes it
    /// </summary>
    Downvote,
    /// <summary>
    /// User unvoted
    /// </summary>
    Unvote
}
