using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Shared.Common.Utils
{
    public static class UsernameGenerateHelperHelper
    {
        private static readonly string[] Animals =
    {
        "lion", "tiger", "panda", "eagle", "wolf", "bear",
        "shark", "falcon", "dragon", "phoenix", "otter", "fox"
    };

        public static string GenerateUsername(string? givenName)
        {
            string baseName;

            if (string.IsNullOrWhiteSpace(givenName))
            {
                // Chọn random con vật
                var random = new Random();
                baseName = Animals[random.Next(Animals.Length)];
            }
            else
            {
                // Chuẩn hóa givenName: lowercase + bỏ ký tự đặc biệt
                baseName = new string(givenName
                    .ToLower()
                    .Normalize(System.Text.NormalizationForm.FormD)
                    .Where(c => char.IsLetterOrDigit(c))
                    .ToArray());

                if (string.IsNullOrEmpty(baseName))
                {
                    // Nếu normalize xong vẫn rỗng → fallback animal
                    var random = new Random();
                    baseName = Animals[random.Next(Animals.Length)];
                }
            }

            // Random 6 số
            var suffix = new Random().Next(100000, 999999);

            return $"{baseName}{suffix}";
        }
    }
}
