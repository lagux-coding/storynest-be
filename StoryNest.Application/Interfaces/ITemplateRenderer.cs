using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface ITemplateRenderer
    {
        string Render(string templateName, IDictionary<string, string> data);
    }
}
