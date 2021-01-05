using JetBrains.Annotations;
using Samples.Clients.Command.CommandResults;

namespace Samples.Clients.Command.Commands
{
    public sealed class CreateClient : GS.DecoupleIt.Operations.Command<CreateClientResult>
    {
        [NotNull]
        public string Name { get; }

        public CreateClient([NotNull] string name)
        {
            Name = name;
        }
    }
}
