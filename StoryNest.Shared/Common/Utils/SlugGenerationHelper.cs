using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StoryNest.Shared.Common.Utils
{
    public static class SlugGenerationHelper
    {
        public static string GenerateSlug(string phrase)
        {
            if (string.IsNullOrWhiteSpace(phrase))
                return string.Empty;

            // chuyển thành lowercase
            string str = phrase.ToLowerInvariant();

            // chuẩn hóa unicode (bỏ dấu tiếng Việt, tiếng khác)
            str = RemoveDiacritics(str);

            // bỏ ký tự không hợp lệ
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");

            // thay nhiều khoảng trắng bằng 1 space
            str = Regex.Replace(str, @"\s+", " ").Trim();

            // cắt slug nếu quá dài (ví dụ 200 char)
            if (str.Length > 200)
                str = str.Substring(0, 200).Trim();

            // thay space bằng dấu -
            str = Regex.Replace(str, @"\s", "-");

            return str;
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
