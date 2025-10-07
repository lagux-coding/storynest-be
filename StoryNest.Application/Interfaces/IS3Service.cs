using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IS3Service
    {
        public string GeneratePresignUrl(string key, string contentType, int expiredMins = 15);
        public Task<string> UploadAIImage(MemoryStream ms);
        public Task<string> UploadAIAudio(MemoryStream ms);
        public Task<string> UploadInvoice(MemoryStream ms, long orderCode, long userId);
    }
}
