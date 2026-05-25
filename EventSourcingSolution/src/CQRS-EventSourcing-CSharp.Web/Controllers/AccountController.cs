using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CQRS_EventSourcing_CSharp.Application.Commands;
using CQRS_EventSourcing_CSharp.Application.CommandHandlers;
using CQRS_EventSourcing_CSharp.Web.DTO;





namespace CQRS_EventSourcing_CSharp.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly OpenAccountHandler _openAccountHandler;
        private readonly DepositMoneyHandler _depositMoneyHandler;
        private readonly WithdrawMoneyHandler _withdrawMoneyHandler;
        private readonly FreezeAccountHandler _freezeAccountHandler;
        private readonly UnfreezeAccountHandler _unfreezeAccountHandler;

        public AccountController(
        OpenAccountHandler openAccountHandler,
        DepositMoneyHandler depositMoneyHandler,
        WithdrawMoneyHandler withdrawMoneyHandler,
        FreezeAccountHandler freezeAccountHandler,
        UnfreezeAccountHandler unfreezeAccountHandler)
        {
            _openAccountHandler = openAccountHandler;
            _depositMoneyHandler = depositMoneyHandler;
            _withdrawMoneyHandler = withdrawMoneyHandler;
            _freezeAccountHandler = freezeAccountHandler;
            _unfreezeAccountHandler = unfreezeAccountHandler;
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
    


        [HttpPost("{accountId:guid}/withdraw")]
        public async Task<IActionResult> Withdraw(Guid accountId, [FromBody] WithdrawMoneyRequest request)
        {
            try
            {
                var command = new WithdrawMoneyCommand
                {
                    AccountId = accountId,
                    Amount = request.Amount,
                    Currency = request.Currency ?? "USD",
                    Description = request.Description ?? "Withdrawal"
                };
                await _withdrawMoneyHandler.Handle(command);
                return Ok(new { message = "Withdrawal successful" });
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


        [HttpPost("{accountId:guid}/freeze")]
        public async Task<IActionResult> Freeze(Guid accountId, [FromBody] FreezeAccountRequest? request)
        {
            try
            {
                var command = new FreezeAccountCommand
                {
                    AccountId = accountId,
                    Reason = request?.Reason ?? string.Empty
                };
                await _freezeAccountHandler.Handle(command);
                return Ok(new { message = "Account frozen" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{accountId:guid}/unfreeze")]
        public async Task<IActionResult> Unfreeze(Guid accountId, [FromBody] UnfreezeAccountRequest? request)
        {
            try
            {
                var command = new UnfreezeAccountCommand
                {
                    AccountId = accountId,
                    Reason = request?.Reason ?? string.Empty
                };
                await _unfreezeAccountHandler.Handle(command);
                return Ok(new { message = "Account unfrozen" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        }
}

