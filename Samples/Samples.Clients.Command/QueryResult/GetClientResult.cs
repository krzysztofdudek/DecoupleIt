using System;

namespace Samples.Clients.Command.QueryResult
{
    public sealed class GetClientResult
    {
        public Guid Id { get; }

        public string Name { get; }

        public GetClientResult(Guid id, string name)
        {
            Id   = id;
            Name = name;
        }
    }
}
