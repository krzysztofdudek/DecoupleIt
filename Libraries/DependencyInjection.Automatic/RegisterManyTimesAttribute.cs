using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.DependencyInjection.Automatic
{
    /// <summary>
    ///     Marks a service type to be registered multiple times instead of being overriden. Class is not inheritable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public sealed class RegisterManyTimesAttribute : Attribute
    {
        /// <summary>
        ///     Marks a service type to be registered multiple times instead of being overriden.
        /// </summary>
        [UsedImplicitly]
        public RegisterManyTimesAttribute() { }
    }
}
