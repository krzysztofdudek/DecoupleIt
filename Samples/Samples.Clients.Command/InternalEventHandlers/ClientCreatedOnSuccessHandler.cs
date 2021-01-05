using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Operations;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Samples.Clients.Command.Contracts.Events;

namespace Samples.Clients.Command.InternalEventHandlers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "LogMessageIsSentenceProblem")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "TemplateIsNotCompileTimeConstantProblem")]
    internal sealed class ClientCreatedOnSuccessHandler : OnSuccessInternalEventHandlerBase<ClientCreated>
    {
        public ClientCreatedOnSuccessHandler([NotNull] ILogger<ClientCreatedOnSuccessHandler> logger)
        {
            _logger = logger;
        }

        protected override ValueTask HandleAsync(ClientCreated @event, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug(@event.ClientId.ToString());

            return new ValueTask();
        }

        [NotNull]
        private readonly ILogger<ClientCreatedOnSuccessHandler> _logger;
    }
}
