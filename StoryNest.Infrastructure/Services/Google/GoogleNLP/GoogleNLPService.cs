using Google.Cloud.Language.V1;
using StoryNest.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Services.Google.GoogleNLP
{
    public class GoogleNLPService : IGoogleNLPService
    {
        private readonly LanguageServiceClient _googleClient;

        public GoogleNLPService(LanguageServiceClient googleClient)
        {
            _googleClient = LanguageServiceClient.Create();
        }
    }
}
