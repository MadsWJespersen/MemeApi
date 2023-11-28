namespace MemeApi.Models.Entity;

public class MemeText : Votable
{
    public string Text { get; set; }
    public MemeTextPosition Position { get; set; }
}
