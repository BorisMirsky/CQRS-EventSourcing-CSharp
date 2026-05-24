using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CQRS_EventSourcing_CSharp.Application.Commands;
using CQRS_EventSourcing_CSharp.Application.CommandHandlers;





namespace CQRS_EventSourcing_CSharp.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly OpenAccountHandler _openAccountHandler;
        private readonly DepositMoneyHandler _depositMoneyHandler;

        public AccountController(OpenAccountHandler openAccountHandler, DepositMoneyHandler depositMoneyHandler)
        {
            _openAccountHandler = openAccountHandler;
            _depositMoneyHandler = depositMoneyHandler;
        }

        [HttpPost("open")]
        public async Task<IActionResult> OpenAccount([FromBody] OpenAccountCommand command)
        {
            try
            {
                await _openAccountHandler.Handle(command);
                return Ok(new { message = "Account opened successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{accountId:guid}/deposit")]
        public async Task<IActionResult> Deposit(Guid accountId, [FromBody] DepositMoneyRequest request)
        {
            try
            {
                var command = new DepositMoneyCommand
                {
                    AccountId = accountId,
                    Amount = request.Amount,
                    Currency = request.Currency ?? "USD",
                    Description = request.Description ?? "Deposit"
                };

                await _depositMoneyHandler.Handle(command);
                return Ok(new { message = "Deposit successful" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class DepositMoneyRequest
    {
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? Description { get; set; }
    }
}
