namespace MemeApi.Models.DTO;

/// <summary>
/// A DTO for a random meme or component.
/// </summary>
/// <param name="data"> The data or content of the component </param>
/// <param name="id"> The components ID </param>
/// <param name="votes"> The vote score of the component </param>
public record RandomComponentDTO(string data, string id, int votes);
