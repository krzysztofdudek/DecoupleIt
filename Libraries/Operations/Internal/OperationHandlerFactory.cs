using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.Operations.Internal
{
    internal static class OperationHandlerFactory
    {
        [NotNull]
        [ItemNotNull]
        public static IEnumerable<ICommandHandler> GetCommandHandlers([NotNull] IServiceProvider serviceProvider, [NotNull] ICommand command)
        {
            return serviceProvider.GetServices(typeof(CommandHandlerBase<>).MakeGenericType(command.GetType()))!.Select(x => (ICommandHandler) x);
        }

        [NotNull]
        [ItemNotNull]
        public static IEnumerable<ICommandWithResultHandler> GetCommandHandlersWithResult(
            [NotNull] IServiceProvider serviceProvider,
            [NotNull] ICommandWithResult commandWithResult)
        {
            var resultType = commandWithResult.ResultType;

            return serviceProvider.GetServices(typeof(CommandHandlerBase<,>).MakeGenericType(commandWithResult.GetType(), resultType))!.Select(
                x => (ICommandWithResultHandler) x);
        }

        [NotNull]
        [ItemNotNull]
        public static IEnumerable<IOnEmissionInternalEventHandler> GetOnEmissionInternalEventHandlers(
            [NotNull] IServiceProvider serviceProvider,
            [NotNull] IInternalEvent internalEvent)
        {
            return serviceProvider.GetServices(typeof(OnEmissionInternalEventHandlerBase<>).MakeGenericType(internalEvent.GetType()))!.Select(
                x => (IOnEmissionInternalEventHandler) x);
        }

        [NotNull]
        [ItemNotNull]
        public static IEnumerable<IOnFailureInternalEventHandler> GetOnFailureInternalEventHandlers(
            [NotNull] IServiceProvider serviceProvider,
            [NotNull] IInternalEvent internalEvent)
        {
            return serviceProvider.GetServices(typeof(OnFailureInternalEventHandlerBase<>).MakeGenericType(internalEvent.GetType()))!.Select(
                x => (IOnFailureInternalEventHandler) x);
        }

        [NotNull]
        [ItemNotNull]
        public static IEnumerable<IOnSuccessInternalEventHandler> GetOnSuccessInternalEventHandlers(
            [NotNull] IServiceProvider serviceProvider,
            [NotNull] IInternalEvent internalEvent)
        {
            return serviceProvider.GetServices(typeof(OnSuccessInternalEventHandlerBase<>).MakeGenericType(internalEvent.GetType()))!.Select(
                x => (IOnSuccessInternalEventHandler) x);
        }

        [NotNull]
        [ItemNotNull]
        public static IEnumerable<IPostCommandHandler> GetPostCommandHandlers([NotNull] IServiceProvider serviceProvider, [NotNull] ICommand command)
        {
            return serviceProvider.GetServices(typeof(PostCommandHandlerBase<>).MakeGenericType(command.GetType()))!.Select(x => (IPostCommandHandler) x);
        }

        [NotNull]
        [ItemNotNull]
        public static IEnumerable<IPostCommandWithResultHandler> GetPostCommandWithResultHandlers(
            [NotNull] IServiceProvider serviceProvider,
            [NotNull] ICommandWithResult commandWithResult)
        {
            var resultType = commandWithResult.ResultType;

            return serviceProvider.GetServices(typeof(PostCommandHandlerBase<,>).MakeGenericType(commandWithResult.GetType(), resultType))!.Select(
                x => (IPostCommandWithResultHandler) x);
        }

        [NotNull]
        [ItemNotNull]
        public static IEnumerable<IPreCommandHandler> GetPreCommandHandlers([NotNull] IServiceProvider serviceProvider, [NotNull] ICommand command)
        {
            return serviceProvider.GetServices(typeof(PreCommandHandlerBase<>).MakeGenericType(command.GetType()))!.Select(x => (IPreCommandHandler) x);
        }

        [NotNull]
        [ItemNotNull]
        public static IEnumerable<IPreCommandWithResultHandler> GetPreCommandWithResultHandlers(
            [NotNull] IServiceProvider serviceProvider,
            [NotNull] ICommandWithResult commandWithResult)
        {
            var resultType = commandWithResult.ResultType;

            return serviceProvider.GetServices(typeof(PreCommandHandlerBase<,>).MakeGenericType(commandWithResult.GetType(), resultType))!.Select(
                x => (IPreCommandWithResultHandler) x);
        }

        [NotNull]
        [ItemNotNull]
        public static IEnumerable<IQueryHandler> GetQueryHandlers([NotNull] IServiceProvider serviceProvider, [NotNull] IQuery query)
        {
            var resultType = query.ResultType;

            return serviceProvider.GetServices(typeof(QueryHandlerBase<,>).MakeGenericType(query.GetType(), resultType))!.Select(x => (IQueryHandler) x);
        }
    }
}
