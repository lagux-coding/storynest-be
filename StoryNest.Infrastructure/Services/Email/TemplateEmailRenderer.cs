using StoryNest.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Services.Email
{
    public sealed class TemplateEmailRenderer : ITemplateRenderer
    {
        private readonly string _basePath;
        private static readonly Regex PlaceHolder = new(@"\{\{([A-Za-z0-9_.-]+)\}\}", RegexOptions.Compiled);

        public TemplateEmailRenderer()
        {
            _basePath = Path.Combine(AppContext.BaseDirectory, "Email", "Templates");
        }

        public string Render(string templateName, IDictionary<string, string> data)
        {
            var path = Path.Combine(_basePath, $"{templateName}.html");

            if (!File.Exists(path))
                throw new FileNotFoundException($"Email template not found: {path}");

            var html = File.ReadAllText(path, Encoding.UTF8);
            return PlaceHolder.Replace(html, m =>
            {
                var key = m.Groups[1].Value;
                return data.TryGetValue(key, out var val) ? val : string.Empty;
            });
        }
    }
}
