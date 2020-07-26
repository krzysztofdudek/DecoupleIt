using System;

namespace Samples.Clients.Command.Contracts.Exceptions
{
    /// <summary>
    ///     Invalid client name exception. Class cannot be inherited.
    /// </summary>
    public sealed class InvalidClientName : Exception
    {
        internal InvalidClientName() { }
    }
}
