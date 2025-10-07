using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using StoryNest.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Services.LogoProvider
{
    public class FileLogoProvider : ILogoProvider
    {
        private readonly IFileProvider _fileProvider;

        public FileLogoProvider(IWebHostEnvironment env)
        {
            _fileProvider = env.WebRootFileProvider;
        }

        public byte[] GetLogo()
        {
            var fileInfo = _fileProvider.GetFileInfo("assets/logo.png");

            if (!fileInfo.Exists)
                throw new FileNotFoundException("Logo file not found", fileInfo.PhysicalPath);

            using var stream = fileInfo.CreateReadStream();
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
