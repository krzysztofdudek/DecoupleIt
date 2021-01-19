using System;

namespace GS.DecoupleIt.HttpAbstraction
{
    /// <summary>
    ///     Marks interface as a http client service. It will be scanned by <see cref="ServiceCollectionExtensions.ScanAssemblyForHttpClients" /> to be implemented by
    ///     <see cref="RestEase" />. Class is not inheritable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public sealed class HttpClientAttribute : Attribute { }
}
