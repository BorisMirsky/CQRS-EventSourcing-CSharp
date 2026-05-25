namespace CQRS_EventSourcing_CSharp.Web.DTO
{
    public class DepositMoneyRequest
    {
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? Description { get; set; }
    }
}
