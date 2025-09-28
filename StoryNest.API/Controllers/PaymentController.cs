using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;
using StoryNest.API.ApiWrapper;
using StoryNest.Application.Interfaces;
using System.Threading.Tasks;

namespace StoryNest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IPayOSPaymentService _payOSPaymenService;
        private readonly ICurrentUserService _currentService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IConfiguration configuration, ILogger<PaymentController> logger, IPayOSPaymentService payOSPaymenService, ICurrentUserService currentService, IPaymentService paymentService)
        {
            _configuration = configuration;
            _logger = logger;
            _payOSPaymenService = payOSPaymenService;
            _currentService = currentService;
            _paymentService = paymentService;
        }

        [Authorize]
        [HttpGet("checkout")]
        public async Task<ActionResult<ApiResponse<object>>> Checkout([FromQuery] int plan)
        {
            var userId = _currentService.UserId;
            if (userId == null || userId <= 0)
            {
                return BadRequest(ApiResponse<object>.Fail("Authentication failed"));
            }

            var result = await _payOSPaymenService.CheckoutAsync(userId.Value, plan);
            return Ok(ApiResponse<object>.Success(result, "Checkout link created successfully"));
        }

        [HttpGet("check-update")]
        public async Task<ActionResult<ApiResponse<object>>> CheckUpdate([FromQuery] long orderCode)
        {
            var userId = _currentService.UserId;
            if (userId == null || userId <= 0)
            {
                return Unauthorized(ApiResponse<object>.Fail("Authentication failed"));
            }

            var result = await _paymentService.GetSuccessPaymentByTXN(userId.Value, orderCode.ToString());
            if (result == null)
            {
                return NotFound(ApiResponse<object>.NotFound("Payment not found"));
            }
            return Ok(ApiResponse<object>.Success(result, "Payment fetched successfully"));
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] WebhookType body)
        {
            var result = await _payOSPaymenService.WebhookAsync(body);
            return Ok("ok");
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
