using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

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

        public static string NormalizeStoryText(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Decode HTML entities
            string text = HttpUtility.HtmlDecode(input);

            // Loại bỏ toàn bộ HTML tag
            text = Regex.Replace(text, "<.*?>", " ", RegexOptions.Singleline);

            // Loại bỏ khoảng trắng thừa, xuống dòng thừa
            text = Regex.Replace(text, @"\s+", " ", RegexOptions.Multiline);

            // Trim khoảng trắng đầu cuối
            text = text.Trim();

            // (optional) Normalize Unicode form — giúp chữ có dấu consistent
            text = text.Normalize(NormalizationForm.FormC);

            return text;
        }
    }
}
