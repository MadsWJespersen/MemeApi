using MemeApi.Models.Entity;
using System.Threading.Tasks;

namespace MemeApi.library.Services.Files;

public interface IMemeRenderingService
{
    public Task<byte[]> RenderMeme(Meme meme);
}
