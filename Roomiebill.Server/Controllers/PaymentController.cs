using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Roomiebill.Server.Models;
using Roomiebill.Server.Services;
using Roomiebill.Server.Services.Interfaces;

namespace Roomiebill.Server.Controllers
{
    /// <summary>
    /// Handles payment processing and debt settlement between users.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IGroupService _groupService;

        public PaymentController(IPaymentService paymentService, IGroupService groupService)
        {
            _paymentService = paymentService;
            _groupService = groupService;
        }

        /// <summary>
        /// Processes a payment between users and updates the corresponding debt.
        /// </summary>
        /// <param name="request">Payment details including amount and user information.</param>
        /// <returns>Success message if payment is processed and debt is updated.</returns>
        /// <response code="200">When payment is processed successfully.</response>
        /// <response code="400">If payment processing fails.</response>
        /// <remarks>
        /// Warning: There is a known issue where debt might not be updated if an error occurs
        /// after successful payment processing but before debt settlement.
        /// </remarks>
        [HttpPost("processPayment")]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
        {
            try
            {
                var result = await _paymentService.ProcessPaymentAsync(request);
                if (!result)
                {
                    return BadRequest(new { Message = "Payment failed." });
                }

                try
                {
                    await _groupService.SettleDebtAsync(request.Amount, request.PayeeInfo, request.PayerInfo, request.GroupId);
                }
                catch (Exception ex)
                {
                    // If debt settlement fails after payment, we should return an error
                    return BadRequest(new { Message = "Payment failed: Unable to settle debt." });
                }

                return Ok(new { Message = "Payment processed successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Payment failed." });
            }
        }
    }
}
