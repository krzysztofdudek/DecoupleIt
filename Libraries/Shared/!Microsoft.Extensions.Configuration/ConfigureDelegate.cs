using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    ///     Delegate used for options configuration.
    /// </summary>
    /// <param name="options">Options.</param>
    /// <typeparam name="TOptions">Type of options.</typeparam>
    public delegate void ConfigureDelegate<in TOptions>([NotNull] TOptions options)
        where TOptions : class;
}
