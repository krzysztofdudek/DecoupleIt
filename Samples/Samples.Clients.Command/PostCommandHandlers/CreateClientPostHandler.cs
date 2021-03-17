using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Operations;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Samples.Clients.Command.CommandResults;
using Samples.Clients.Command.Commands;
using Samples.Clients.Command.Contracts.Events;

namespace Samples.Clients.Command.PostCommandHandlers
{
    internal sealed class CreateClientPostHandler : PostCommandHandlerBase<CreateClient, CreateClientResult>
    {
        public CreateClientPostHandler([NotNull] ILogger<CreateClientPostHandler> logger)
        {
            _logger = logger;
        }

        protected override ValueTask PostHandleAsync(
            CreateClient command,
            CreateClientResult result,
            IReadOnlyCollection<InternalEvent> internalEvents,
            Exception exception,
            CancellationToken cancellationToken = default)
        {
            if (internalEvents.OfType<ClientCreated>()
                              .Any())
                // ReSharper disable once LogMessageIsSentenceProblem
                _logger.LogInformation("Client created.");

            return new ValueTask();
        }

        [NotNull]
        private readonly ILogger<CreateClientPostHandler> _logger;
    }
}
