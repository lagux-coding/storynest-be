using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using StoryNest.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Services.S3
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucket;

        public S3Service()
        {
            // Get from env
            var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY");
            var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_KEY");
            var region = Environment.GetEnvironmentVariable("AWS_REGION");
            var bucketName = Environment.GetEnvironmentVariable("AWS_BUCKET_NAME");

            if (string.IsNullOrEmpty(accessKey) ||
                string.IsNullOrEmpty(secretKey) ||
                string.IsNullOrEmpty(region) ||
                string.IsNullOrEmpty(bucketName))
            {
                throw new Exception("ENV AWS S3 is not set");
            }

            _bucket = bucketName;
            _s3Client = new AmazonS3Client(accessKey, secretKey, Amazon.RegionEndpoint.GetBySystemName(region));
        }

        public string GeneratePresignUrl(string key, string contentType, int expiredMins = 15)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucket,
                Key = key,
                Expires = DateTime.UtcNow.AddMinutes(expiredMins),
                Verb = HttpVerb.PUT,
                ContentType = contentType,
            };

            return _s3Client.GetPreSignedURL(request);
        }

        public async Task<string> UploadAIImage(MemoryStream ms)
        {
            string guid = Guid.NewGuid().ToString("N");
            var request = new PutObjectRequest
            {
                BucketName = _bucket,
                Key = $"generated-content/image/img_{guid}.webp",
                InputStream = ms,
                ContentType = "image/png"
            };

            await _s3Client.PutObjectAsync(request);

            return request.Key;
        }

        public async Task<string> UploadAIAudio(MemoryStream ms)
        {
            string guid = Guid.NewGuid().ToString("N");
            var request = new PutObjectRequest
            {
                BucketName = _bucket,
                Key = $"generated-content/audio/aud_{guid}.wav",
                InputStream = ms,
                ContentType = "audio/wav"
            };

            await _s3Client.PutObjectAsync(request);

            return request.Key;
        }
    }
}
