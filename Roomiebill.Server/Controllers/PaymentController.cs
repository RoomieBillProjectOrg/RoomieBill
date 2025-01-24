using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Roomiebill.Server.Models;
using Roomiebill.Server.Services;

namespace Roomiebill.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly GroupService _groupService;

        public PaymentController(IPaymentService paymentService, GroupService groupService)
        {
            _paymentService = paymentService;
            _groupService = groupService;
        }

        [HttpPost("processPayment")]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
        {
            var result = await _paymentService.ProcessPaymentAsync(request);
            if (result)
            {
                //TODO: It is a problematic place where the user can pay but the debt is not updated because of error.
                await _groupService.SettleDebtAsync(request.Amount, request.PayeeInfo, request.PayerInfo, request.GroupId);
                return Ok(new { Message = "Payment processed successfully." });
            }
            return BadRequest(new { Message = "Payment failed." });
        }
    }
}
