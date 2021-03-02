using System;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Operations;
using Samples.Clients.Command.CommandResults;
using Samples.Clients.Command.Commands;

namespace Samples.Clients.Command.PreCommandHandler
{
    internal sealed class CreateClientPostHandler : PreCommandHandlerBase<CreateClient, CreateClientResult>
    {
        protected override ValueTask HandleAsync(CreateClient command, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(command.Name))
                throw new InvalidOperationException("Name cannot be null or empty.").WithCategory(ExceptionCategories.Business);

            return new ValueTask();
        }
    }
}
