using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IUploadService
    {
        public Task<UploadMediaResponse> UploadMedia(UploadMediaRequest request, long userId);
    }
}
