using Ganss.Xss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using HtmlAgilityPack;

namespace StoryNest.Shared.Common.Utils
{
    public static class SummaryHelper
    {
        public static string Generate(string htmlContent, int maxLength = 200)
        {
            if (string.IsNullOrWhiteSpace(htmlContent))
                return string.Empty;

            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            var sb = new StringBuilder();

            // 1. Lấy heading đầu tiên (h1-h3)
            var heading = doc.DocumentNode.SelectSingleNode("//h1|//h2|//h3");
            if (heading != null)
                sb.Append(heading.OuterHtml);

            // 2. Lấy đoạn <p> đầu tiên
            var paragraph = doc.DocumentNode.SelectSingleNode("//p");
            if (paragraph != null)
            {
                var text = paragraph.InnerText.Trim();

                if (text.Length > maxLength)
                {
                    var truncated = text.Substring(0, maxLength);
                    var lastSpace = truncated.LastIndexOf(' ');
                    if (lastSpace > 0) truncated = truncated.Substring(0, lastSpace);

                    sb.Append($"<p>{truncated}...</p>");
                }
                else
                {
                    sb.Append(paragraph.OuterHtml);
                }
            }

            // 3. Sanitize để tránh XSS, chỉ cho phép tag cơ bản
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedTags.Clear();
            sanitizer.AllowedTags.Add("h1");
            sanitizer.AllowedTags.Add("h2");
            sanitizer.AllowedTags.Add("h3");
            sanitizer.AllowedTags.Add("p");
            sanitizer.AllowedTags.Add("b");
            sanitizer.AllowedTags.Add("i");
            sanitizer.AllowedTags.Add("strong");
            sanitizer.AllowedTags.Add("em");
            sanitizer.AllowedTags.Add("ul");
            sanitizer.AllowedTags.Add("li");
            sanitizer.AllowedTags.Add("a");

            return sanitizer.Sanitize(sb.ToString());
        }
    }
}
