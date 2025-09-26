using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;
using StoryNest.API.ApiWrapper;
using StoryNest.Application.Interfaces;

namespace StoryNest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IPayOSPaymentService _payOSPaymenService;
        private readonly ICurrentUserService _currentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IConfiguration configuration, ILogger<PaymentController> logger, IPayOSPaymentService payOSPaymenService, ICurrentUserService currentService)
        {
            _configuration = configuration;
            _logger = logger;
            _payOSPaymenService = payOSPaymenService;
            _currentService = currentService;
        }

        [HttpGet("checkout/{planId}")]
        public async Task<ActionResult<ApiResponse<object>>> Checkout(int planId)
        {
            var userId = _currentService.UserId;
            if (userId == null || userId <= 0)
            {
                return BadRequest(ApiResponse<object>.Fail("Authentication failed"));
            }

            var result = await _payOSPaymenService.CheckoutAsync(userId.Value, planId);
            return Ok(ApiResponse<object>.Success(result, "Checkout link created successfully"));
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
            var userId = _currentService.UserId;
            if (userId == null || userId <= 0)
            {
                return BadRequest(ApiResponse<object>.Fail("Authentication failed"));
            }

            var result = await _payOSPaymenService.CancelAsync(userId.Value, orderCode);
            if (!result)
            {
                return BadRequest(ApiResponse<object>.Fail("Failed to cancel payment"));
            }

            return Ok(ApiResponse<object>.Success(result, "Payment cancelled successfully"));
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
