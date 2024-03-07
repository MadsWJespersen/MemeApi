using Amazon.S3.Model;
using Amazon.S3;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System;

namespace MemeApi.library.Services.Files
{
    public class S3FileStorageClient : IFileSaver, IFileRemover
    {
        private readonly MemeApiSettings _settings;
        private readonly AmazonS3Client _client;

        public S3FileStorageClient(MemeApiSettings settings)
        {
            _settings = settings;

            string accessKey = _settings.GetBlobStorageAccessKey();
            string secretKey = _settings.GetBlobStorageAccessKeySecret();

            AmazonS3Config config = new AmazonS3Config();
            config.ServiceURL = _settings.GetBlobStorageServiceUrl();

            _client = new AmazonS3Client(
                accessKey,
                secretKey,
                config
            );
        }

        public async Task SaveFile(IFormFile file, string path, string fileName)
        {

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var putRequest = new PutObjectRequest
                {
                    BucketName = _settings.GetBlobStorageBucketName(),
                    Key = path + fileName,
                    InputStream = memoryStream,
                    ContentType = file.ContentType,
                    CannedACL = S3CannedACL.PublicRead
                };

                var response = await _client.PutObjectAsync(putRequest);

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
        public async Task RemoveFile(string path)
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _settings.GetBlobStorageBucketName(),
                Key = path
            };

            var response = await _client.DeleteObjectAsync(deleteRequest);

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
