using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace MemeApi.library.Services.Files;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class FileSaver : IFileSaver
{
    private readonly IConfiguration _configuration;
    public FileSaver(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SaveFile(IFormFile file, string path, string fileName)
    {
        var completePath = Path.Combine(_configuration["BaseUploadFolder"], path, fileName);
        using (Stream fileStream = new FileStream(completePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        };
    }
}
