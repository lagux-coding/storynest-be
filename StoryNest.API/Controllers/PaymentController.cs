using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;
using StoryNest.API.ApiWrapper;

namespace StoryNest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IConfiguration configuration, ILogger<PaymentController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("checkout/{planId}")]
        public async Task<ActionResult<ApiResponse<object>>> Checkout(int planId)
        {
            var clientId = _configuration["PAYOS_CLIENT_ID"];
            var apiKey = _configuration["PAYOS_API_KEY"];
            var checksum = _configuration["PAYOS_CHECKSUM"];

            PayOS payos = new PayOS(clientId, apiKey, checksum);
            ItemData item = new ItemData("Bloom", 1, 4000);
            List<ItemData> items = new List<ItemData>();
            items.Add(item);

            long expiredAt = new DateTimeOffset(DateTime.UtcNow.AddMinutes(10))
                    .ToUnixTimeSeconds();

            PaymentData paymentData = new PaymentData(3335, 4000, "StoryNest", items, "https://dev.storynest.io.vn", "https://dev.storynest.io.vn", expiredAt: expiredAt);


            CreatePaymentResult createPayment = await payos.createPaymentLink(paymentData);
            return Ok(ApiResponse<object>.Success(createPayment, "Checkout link created successfully"));
        }

        [HttpPost("webhook")]
        public IActionResult Webhook([FromBody] WebhookType body)
        {
            try
            {
                var clientId = _configuration["PAYOS_CLIENT_ID"];
                var apiKey = _configuration["PAYOS_API_KEY"];
                var checksum = _configuration["PAYOS_CHECKSUM"];

                PayOS _payOS = new PayOS(clientId, apiKey, checksum);
                WebhookData data = _payOS.verifyPaymentWebhookData(body);


                if (data.code == "00")
                {
                    _logger.LogInformation("✅ Webhook success. Order {OrderCode}, Amount {Amount}", data.orderCode, data.amount);
                    return Ok("ok");
                }
                else
                {
                    return Ok("fail");
                }

                return Ok("ok");
            }
            catch (Exception ex)
            {
                return Ok("error");
            }
        }


        [HttpGet("cancel")]
        public async Task<ActionResult<ApiResponse<object>>> Cancel(long orderCode)
        {
            var clientId = _configuration["PAYOS_CLIENT_ID"];
            var apiKey = _configuration["PAYOS_API_KEY"];
            var checksum = _configuration["PAYOS_CHECKSUM"];

            PayOS payos = new PayOS(clientId, apiKey, checksum);
            PaymentLinkInformation cancelledPaymentLinkInfo = await payos.cancelPaymentLink(orderCode, "User cancel");

            return Ok(ApiResponse<object>.Success(cancelledPaymentLinkInfo, "Payment cancelled successfully"));
        }

        [HttpGet("confirm-webhook")]
        public async Task<ActionResult<ApiResponse<object>>> Confirm([FromQuery] string url)
        {
            var clientId = _configuration["PAYOS_CLIENT_ID"];
            var apiKey = _configuration["PAYOS_API_KEY"];
            var checksum = _configuration["PAYOS_CHECKSUM"];

            PayOS payos = new PayOS(clientId, apiKey, checksum);
            var result = await payos.confirmWebhook(url);

            return Ok(ApiResponse<object>.Success(result, "Webhook url confirm successfully"));
        }
    }
}
