using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StoryNest.Shared.Common.Utils
{
    public static class TextNormalizer
    {
        public static string Normalize(string input, bool keepUnderscore = false)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // 1️ Lowercase toàn bộ
            var normalized = input.ToLowerInvariant();

            // 2️ Xóa emoji và ký tự không phải ký tự chữ, số, tiếng Việt
            // Giữ lại chữ cái tiếng Việt (có dấu), số, khoảng trắng, và tùy chọn dấu "_"
            string pattern = keepUnderscore
                ? @"[^a-z0-9_àáạảãâầấậẩẫăằắặẳẵèéẹẻẽêềếệểễìíịỉĩòóọỏõôồốộổỗơờớợởỡùúụủũưừứựửữỳýỵỷỹđ\s]"
                : @"[^a-z0-9àáạảãâầấậẩẫăằắặẳẵèéẹẻẽêềếệểễìíịỉĩòóọỏõôồốộổỗơờớợởỡùúụủũưừứựửữỳýỵỷỹđ\s]";

            normalized = Regex.Replace(normalized, pattern, " ");

            // 3️ Chuẩn hóa khoảng trắng (xóa trùng, trim hai đầu)
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

            return normalized;
        }
    }
}
