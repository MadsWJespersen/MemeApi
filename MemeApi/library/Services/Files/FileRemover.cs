using System.IO;
using System.Threading.Tasks;

namespace MemeApi.library.Services.Files;

public class FileRemover : IFileRemover
{
    public Task RemoveFile(string path)
    {
        File.Delete(path);
        return Task.CompletedTask;
    }
}