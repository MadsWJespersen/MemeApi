using System.Threading.Tasks;

namespace MemeApi.library.Services.Files
{
    public interface IFileLoader
    {
        Task<byte[]> LoadFile(string path);
    }
}
