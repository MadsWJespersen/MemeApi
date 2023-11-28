using System.IO;

namespace MemeApi.library.Services.Files;

public class FileRemover : IFileRemover
{
    public void RemoveFile(string path)
    {
        File.Delete(path);
    }
}