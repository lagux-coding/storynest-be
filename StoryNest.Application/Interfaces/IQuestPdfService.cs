using StoryNest.Application.Dtos.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IQuestPdfService
    {
        byte[] Generate(InvoiceDto invoice);
    }
}
