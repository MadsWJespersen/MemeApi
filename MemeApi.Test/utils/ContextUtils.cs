using MemeApi.Models.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using MemeApi.library;

namespace MemeApi.Test.utils
{
    internal class ContextUtils
    {
        public static MemeContext CreateMemeTestContext(MemeApiSettings settings)
        {
            var contextOptions = new DbContextOptionsBuilder<MemeContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            return new MemeContext(contextOptions, settings);
        }
    }
}
