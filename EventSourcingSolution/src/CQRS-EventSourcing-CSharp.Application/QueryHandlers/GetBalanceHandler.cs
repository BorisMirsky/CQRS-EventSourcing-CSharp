using CQRS_EventSourcing_CSharp.Application.Abstractions;
using CQRS_EventSourcing_CSharp.Application.DTO;
using CQRS_EventSourcing_CSharp.Application.Queries;
using System;
using System.Collections.Generic;
using System.Text;





namespace CQRS_EventSourcing_CSharp.Application.QueryHandlers
{
    public class GetBalanceHandler
    {
        private readonly IReadModelRepository _readModelRepository;

        public GetBalanceHandler(IReadModelRepository readModelRepository)
        {
            _readModelRepository = readModelRepository;
        }

        public async Task<AccountBalanceReadDTO?> Handle(GetBalanceQuery query, CancellationToken cancellationToken)
        {
            return await _readModelRepository.GetAccountBalance(query.AccountId, cancellationToken);
        }
    }
}
