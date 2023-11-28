using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MemeApi.library.Services.Files;

public interface IFileSaver
{
    Task SaveFile(IFormFile file, string path, string fileName);
}
