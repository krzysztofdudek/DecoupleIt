using System;
using System.Collections.Generic;
using System.Linq;
using GS.DecoupleIt.DependencyInjection.Automatic;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.Operations.Internal
{
    [Singleton]
    internal sealed class OperationHandlerFactory
    {
        public OperationHandlerFactory([NotNull] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<ICommandHandler> GetCommandHandlers([NotNull] ICommand command)
        {
            return _serviceProvider.GetServices(typeof(CommandHandlerBase<>).MakeGenericType(command.GetType()))!.Select(x => (ICommandHandler) x);
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<ICommandWithResultHandler> GetCommandHandlersWithResult([NotNull] ICommandWithResult commandWithResult)
        {
            var resultType = commandWithResult.ResultType;

            return _serviceProvider.GetServices(typeof(CommandHandlerBase<,>).MakeGenericType(commandWithResult.GetType(), resultType))!.Select(
                x => (ICommandWithResultHandler) x);
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<IOnEmissionInternalEventHandler> GetOnEmissionInternalEventHandlers([NotNull] IInternalEvent internalEvent)
        {
            return _serviceProvider.GetServices(typeof(OnEmissionInternalEventHandlerBase<>).MakeGenericType(internalEvent.GetType()))!.Select(
                x => (IOnEmissionInternalEventHandler) x);
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<IOnFailureInternalEventHandler> GetOnFailureInternalEventHandlers([NotNull] IInternalEvent internalEvent)
        {
            return _serviceProvider.GetServices(typeof(OnFailureInternalEventHandlerBase<>).MakeGenericType(internalEvent.GetType()))!.Select(
                x => (IOnFailureInternalEventHandler) x);
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<IOnSuccessInternalEventHandler> GetOnSuccessInternalEventHandlers([NotNull] IInternalEvent internalEvent)
        {
            return _serviceProvider.GetServices(typeof(OnSuccessInternalEventHandlerBase<>).MakeGenericType(internalEvent.GetType()))!.Select(
                x => (IOnSuccessInternalEventHandler) x);
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<IPostCommandHandler> GetPostCommandHandlers([NotNull] ICommand command)
        {
            return _serviceProvider.GetServices(typeof(PostCommandHandlerBase<>).MakeGenericType(command.GetType()))!.Select(x => (IPostCommandHandler) x);
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<IPostCommandWithResultHandler> GetPostCommandWithResultHandlers([NotNull] ICommandWithResult commandWithResult)
        {
            var resultType = commandWithResult.ResultType;

            return _serviceProvider.GetServices(typeof(PostCommandHandlerBase<,>).MakeGenericType(commandWithResult.GetType(), resultType))!.Select(
                x => (IPostCommandWithResultHandler) x);
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<IPreCommandHandler> GetPreCommandHandlers([NotNull] ICommand command)
        {
            return _serviceProvider.GetServices(typeof(PreCommandHandlerBase<>).MakeGenericType(command.GetType()))!.Select(x => (IPreCommandHandler) x);
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<IPreCommandWithResultHandler> GetPreCommandWithResultHandlers([NotNull] ICommandWithResult commandWithResult)
        {
            var resultType = commandWithResult.ResultType;

            return _serviceProvider.GetServices(typeof(PreCommandHandlerBase<,>).MakeGenericType(commandWithResult.GetType(), resultType))!.Select(
                x => (IPreCommandWithResultHandler) x);
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<IQueryHandler> GetQueryHandlers([NotNull] IQuery query)
        {
            var resultType = query.ResultType;

            return _serviceProvider.GetServices(typeof(QueryHandlerBase<,>).MakeGenericType(query.GetType(), resultType))!.Select(x => (IQueryHandler) x);
        }

        [NotNull]
        private readonly IServiceProvider _serviceProvider;
    }
}
