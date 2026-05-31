using CQRS_EventSourcing_CSharp.Application.Abstractions;
using CQRS_EventSourcing_CSharp.Application.DTO;
using CQRS_EventSourcing_CSharp.Application.Queries;
using System;
using System.Collections.Generic;
using System.Text;



namespace CQRS_EventSourcing_CSharp.Application.QueryHandlers
{
    public class GetTransactionHistoryHandler
    {
        private readonly IReadModelRepository _readModelRepository;

        public GetTransactionHistoryHandler(IReadModelRepository readModelRepository)
        {
            _readModelRepository = readModelRepository;
        }

        public async Task<IEnumerable<TransactionHistoryReadDTO>> Handle(GetTransactionHistoryQuery query, CancellationToken cancellationToken)
        {
            return await _readModelRepository.GetTransactionHistory(query.AccountId, cancellationToken);
        }
    }

}
