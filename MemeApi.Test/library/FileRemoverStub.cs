using MemeApi.library.Services.Files;
using System.Threading.Tasks;

namespace MemeApi.Test.library
{
    internal class FileRemoverStub : IFileRemover
    {
        public Task RemoveFile(string path)
        {
            return Task.CompletedTask;
        }
    }
}
