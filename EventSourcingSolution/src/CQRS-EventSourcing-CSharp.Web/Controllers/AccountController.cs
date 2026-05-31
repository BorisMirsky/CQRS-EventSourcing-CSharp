using CQRS_EventSourcing_CSharp.Application.CommandHandlers;
using CQRS_EventSourcing_CSharp.Application.Commands;
using CQRS_EventSourcing_CSharp.Application.Queries;
using CQRS_EventSourcing_CSharp.Application.QueryHandlers;
using CQRS_EventSourcing_CSharp.Web.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;





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
        private readonly GetBalanceHandler _getBalanceHandler;
        private readonly GetBalanceOnDateHandler _getBalanceOnDateHandler;
        private readonly GetTransactionHistoryHandler _getTransactionHistoryHandler;
        //private readonly CancellationToken cancellationToken;

        public AccountController(
            OpenAccountHandler openAccountHandler,
            DepositMoneyHandler depositMoneyHandler,
            WithdrawMoneyHandler withdrawMoneyHandler,
            FreezeAccountHandler freezeAccountHandler,
            UnfreezeAccountHandler unfreezeAccountHandler,
            GetBalanceHandler getBalanceHandler,
            GetBalanceOnDateHandler getBalanceOnDateHandler,
            GetTransactionHistoryHandler getTransactionHistoryHandler)
        {
            _openAccountHandler = openAccountHandler;
            _depositMoneyHandler = depositMoneyHandler;
            _withdrawMoneyHandler = withdrawMoneyHandler;
            _freezeAccountHandler = freezeAccountHandler;
            _unfreezeAccountHandler = unfreezeAccountHandler;
            _getBalanceHandler = getBalanceHandler;
            _getBalanceOnDateHandler = getBalanceOnDateHandler;
            _getTransactionHistoryHandler = getTransactionHistoryHandler;
        }

        [HttpPost("open")]
        public async Task<IActionResult> OpenAccount([FromBody] OpenAccountCommand command)
        {
            try
            {
                await _openAccountHandler.Handle(command, CancellationToken.None);
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
                await _withdrawMoneyHandler.Handle(command, CancellationToken.None);
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
                await _freezeAccountHandler.Handle(command, CancellationToken.None);
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
                await _unfreezeAccountHandler.Handle(command, CancellationToken.None);
                return Ok(new { message = "Account unfrozen" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpGet("{accountId:guid}/balance")]
        public async Task<IActionResult> GetBalance(Guid accountId)
        {
            var query = new GetBalanceQuery { AccountId = accountId };
            var result = await _getBalanceHandler.Handle(query, CancellationToken.None);
            if (result == null)
                return NotFound(new { error = "Account not found" });
            return Ok(result);
        }


        [HttpGet("{accountId:guid}/history")]
        public async Task<IActionResult> GetHistory(Guid accountId)
        {
            var query = new GetTransactionHistoryQuery { AccountId = accountId };
            var result = await _getTransactionHistoryHandler.Handle(query, CancellationToken.None);
            return Ok(result);
        }


        [HttpGet("{accountId:guid}/balance-at-date")]
        public async Task<IActionResult> GetBalanceOnDate(Guid accountId, [FromQuery] DateTime date)
        {
            var query = new GetBalanceOnDateQuery { AccountId = accountId, Date = date };
            var balance = await _getBalanceOnDateHandler.Handle(query, CancellationToken.None);
            if (balance == null)
                return NotFound(new { error = "No events found for this account before the date" });
            return Ok(new { accountId, balanceAmount = balance.Amount, currency = balance.Currency, asOfDate = date });
        }

    }
}

