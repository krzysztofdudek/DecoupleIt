using System;
using GS.DecoupleIt.Operations;
using Samples.Clients.Command.QueryResult;

namespace Samples.Clients.Command.Queries
{
    public sealed class GetClient : Query<GetClientResult>
    {
        public Guid Id { get; }

        public GetClient(Guid id)
        {
            Id = id;
        }
    }
}
