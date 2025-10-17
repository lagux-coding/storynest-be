using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Shared.Common.Utils
{
    public static class OffensiveWordLoader
    {
        public static HashSet<string> LoadWords(string path)
        {
            if (!File.Exists(path)) 
                throw new FileNotFoundException($"The file at path {path} was not found.");

            var words = File.ReadLines(path)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l.Trim().ToLowerInvariant());

            return new HashSet<string>(words);
        }
    }
}
