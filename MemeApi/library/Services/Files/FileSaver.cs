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

    public async Task SavseFile(IFormFile file, string path, string fileName)
    {
        var completePath = Path.Combine(_settings.GetBaseUploadFolder(), path, fileName);
        using (Stream fileStream = new FileStream(completePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        };
    }

    public async Task SaveFile(IFormFile file, string path, string fileName)
    {
        string accessKey = _settings.GetBlobStorageAccessKeySecret();
        string secretKey = _settings.GetBlobStorageAccessKeySecret();

        AmazonS3Config config = new AmazonS3Config();
        config.ServiceURL = _settings.GetBlobStorageServiceUrl();
        config.RegionEndpoint = RegionEndpoint.EUCentral1;

        AmazonS3Client s3Client = new AmazonS3Client(
            accessKey,
            secretKey,
            config
        );

        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var putRequest = new PutObjectRequest
            {
                BucketName = path,
                Key = fileName,
                InputStream = memoryStream,
                ContentType = file.ContentType
            };

            var response = await s3Client.PutObjectAsync(putRequest);

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("File uploaded successfully");
            }
            else
            {
                Console.WriteLine($"Error uploading file. Status code: {response.HttpStatusCode}");
            }
        }

    }
}
