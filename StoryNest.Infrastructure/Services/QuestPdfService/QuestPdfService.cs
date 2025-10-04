using QuestPDF.Fluent;
using StoryNest.Application.Dtos.Dto;
using StoryNest.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Services.QuestPdfService
{
    public class QuestPdfService : IQuestPdfService
    {
        private readonly ILogoProvider _logoProvider;

        public QuestPdfService(ILogoProvider logoProvider)
        {
            _logoProvider = logoProvider;
        }

        public byte[] Generate(InvoiceDto invoice)
        {
            var doc = new InvoiceDocument(invoice, _logoProvider);
            using var ms = new MemoryStream();
            doc.GeneratePdf(ms);
            return ms.ToArray();
        }
    }
}
