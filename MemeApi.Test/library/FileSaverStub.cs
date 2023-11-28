using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemeApi.library.Services.Files;
using Microsoft.AspNetCore.Http;

namespace MemeApi.Test.library
{
    internal class FileSaverStub : IFileSaver
    {
        public Task SaveFile(IFormFile file, string path, string fileName)
        {
            return Task.CompletedTask;
        }
    }
}
