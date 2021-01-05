using System;

namespace Samples.Clients.Command.CommandResults
{
    public sealed class CreateClientResult
    {
        public Guid Id { get; }

        public string Name { get; }

        public CreateClientResult(Guid id, string name)
        {
            Id   = id;
            Name = name;
        }
    }
}
