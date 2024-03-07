using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;

namespace MemeApi.library.Services.Files;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class FileSaver : IFileSaver
{
    private readonly MemeApiSettings _settings;
    public FileSaver(MemeApiSettings settings)
    {
        _settings = settings;
    }

    public async Task SaveFile(IFormFile file, string path, string fileName)
    {
        var completePath = Path.Combine(_settings.GetBaseUploadFolder(), path, fileName);
        using (Stream fileStream = new FileStream(completePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        };
    }
}
