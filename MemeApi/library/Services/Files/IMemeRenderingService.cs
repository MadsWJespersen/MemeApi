using MemeApi.Models.Entity;

namespace MemeApi.library.Services.Files;

public interface IMemeRenderingService
{
    public byte[] RenderMeme(Meme meme);
}
