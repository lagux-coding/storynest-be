using AngleSharp.Dom;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StoryNest.Application.Dtos.Dto;
using StoryNest.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDocument = QuestPDF.Infrastructure.IDocument;

namespace StoryNest.Infrastructure.Services.QuestPdfService
{
    public class InvoiceDocument : IDocument
    {
        private readonly InvoiceDto _invoice;
        private readonly ILogoProvider _logoProvider;

        public InvoiceDocument(InvoiceDto invoice, ILogoProvider logoProvider)
        {
            _invoice = invoice;
            _logoProvider = logoProvider;
        }

        public DocumentMetadata GetMetadata => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            var logoBytes = _logoProvider.GetLogo();

            var fontPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "assets");

            FontManager.RegisterFont(File.OpenRead(Path.Combine(fontPath, "Roboto-Regular.ttf")));
            FontManager.RegisterFont(File.OpenRead(Path.Combine(fontPath, "Roboto-Bold.ttf")));
            FontManager.RegisterFont(File.OpenRead(Path.Combine(fontPath, "Roboto-Light.ttf")));
            FontManager.RegisterFont(File.OpenRead(Path.Combine(fontPath, "Roboto-Italic.ttf")));

            container
                .Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    page.DefaultTextStyle(x => x.FontFamily("Roboto").FontSize(12));

                    // Header
                    page.Header().PaddingBottom(20).Row(row =>
                    {
                        row.ConstantItem(100).Height(50).Image(logoBytes).FitHeight();
                    });

                    // Content
                    page.Content().Column(col =>
                    {

                        col.Item().PaddingBottom(20).Text("Hoá đơn gói đăng ký StoryNest").FontSize(20).Light();

                        col.Item().Column(subCol =>
                        {
                            subCol.Spacing(20);
                            subCol.Item().Row(row =>
                            {
                                row.RelativeItem().Column(fromCol =>
                                {
                                    // From
                                    fromCol.Item().PaddingBottom(10).Text("Từ").Bold();
                                    fromCol.Item().Text("StoryNest").Light();
                                    fromCol.Item().Text("Ho Chi Minh City, Viet Nam").Light();
                                    fromCol.Item().Text("support@storynest.io.vn").Light();
                                });

                                row.RelativeItem().Column(toCol =>
                                {
                                    // Subscription
                                    toCol.Item().PaddingBottom(10).Text("Chi tiết hoá đơn").Bold();
                                    toCol.Item().Table(table =>
                                    {
                                        table.ColumnsDefinition(c =>
                                        {
                                            c.RelativeColumn();
                                            c.RelativeColumn();
                                        });

                                        table.Cell().Text("Số hoá đơn").Light(); table.Cell().Text($"{_invoice.OrderCode}").Light();
                                        table.Cell().Text("Ngày phát hành").Light(); table.Cell().Text(_invoice.IssueDate.ToString("dd/MM/yyyy HH:mm")).Light();
                                        table.Cell().Text("Ngày thanh toán").Light(); table.Cell().Text(_invoice.Payment.PaidAt.Value.ToString("dd/MM/yyyy HH:mm")).Light();
                                    });
                                });
                            });

                            // Billing To
                            subCol.Item().Row(row =>
                            {
                                row.RelativeItem().Column(billTo =>
                                {
                                    billTo.Item().PaddingBottom(10).Text("Khách hàng").Bold();
                                    billTo.Item().Text(_invoice.User.FullName).Light();
                                    billTo.Item().Text(_invoice.User.Email).Light();
                                });

                                row.RelativeItem().Column(payment =>
                                {
                                    // Payment
                                    payment.Item().PaddingBottom(10).Text("Chi tiết thanh toán").Bold();
                                    payment.Item().Table(table =>
                                    {
                                        table.ColumnsDefinition(c =>
                                        {
                                            c.RelativeColumn();
                                            c.RelativeColumn();
                                        });

                                        table.Cell().Text("Nhà cung cấp").Light(); table.Cell().Text("VietQR").Light();
                                        table.Cell().Text("Mã giao dịch").Light(); table.Cell().Text(_invoice.Payment.ProviderTXN).Light();
                                        table.Cell().Text("Trạng thái").Light(); table.Cell().Text("Thành công").Light();
                                        table.Cell().Text("Currency").Light(); table.Cell().Text(_invoice.Payment.Currency).Light();
                                    });
                                });
                            });

                            // Summary
                            subCol.Item().PaddingTop(10).Text("Tóm tắt").Light().FontSize(20);
                            subCol.Item().LineHorizontal(1).LineColor(Colors.Black);
                            subCol.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn();
                                    c.RelativeColumn();
                                });

                                table.Cell().PaddingBottom(5).Text("Tổng số tiền").Light(); table.Cell().PaddingBottom(5).AlignRight().Text(_invoice.Amount.ToString()).Light();
                                table.Cell().Text("Giảm giá").Light(); table.Cell().AlignRight().Text("0 VND").Light();
                            });
                            subCol.Item().LineHorizontal(1).LineColor(Colors.Black);
                            subCol.Item().PaddingBottom(20).Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn();
                                    c.RelativeColumn();
                                });

                                table.Cell().Text("Tổng số tiền phải trả").FontSize(20).Bold(); table.Cell().AlignRight().Text(_invoice.Amount.ToString()).FontSize(20).Bold();                               
                            });
                            subCol.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                            subCol.Item().Column(detail =>
                            {
                                detail.Item().PaddingTop(10).PaddingBottom(5).Text("Chi tiết gói đăng ký").FontSize(20).Light();
                                detail.Item().Text("Dưới đây là chi tiết gói đăng ký của bạn").Italic().Light();
                            });
                            subCol.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn();   
                                    c.ConstantColumn(80);
                                    c.ConstantColumn(100);
                                    c.ConstantColumn(100);
                                    c.ConstantColumn(80);
                                });

                                // Header
                                table.Cell().PaddingBottom(10).PaddingTop(10).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Text("Gói dịch vụ").SemiBold();
                                table.Cell().PaddingBottom(10).PaddingTop(10).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).AlignCenter().Text("Thời hạn").SemiBold();
                                table.Cell().PaddingBottom(10).PaddingTop(10).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).AlignCenter().Text("Bắt đầu").SemiBold();
                                table.Cell().PaddingBottom(10).PaddingTop(10).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).AlignCenter().Text("Kết thúc").SemiBold();
                                table.Cell().PaddingBottom(10).PaddingTop(10).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).AlignRight().Text("Giá").SemiBold();

                                // Data
                                table.Cell().Text(_invoice.Subscription.Plan.Name);
                                table.Cell().AlignCenter().Text(_invoice.Subscription.Plan.DurationInDays.ToString());
                                table.Cell().AlignCenter().Text(_invoice.Subscription.StartDate.ToString("dd/MM/yyyy HH:mm"));
                                table.Cell().AlignCenter().Text(_invoice.Subscription.EndDate.ToString("dd/MM/yyyy HH:mm"));
                                table.Cell().AlignRight().Text(_invoice.Amount.ToString());
                            });
                        });
                    });
                });
        }
    }
}
