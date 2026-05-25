namespace CQRS_EventSourcing_CSharp.Web.DTO
{
    public class WithdrawMoneyRequest
    {
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? Description { get; set; }
    }
}
