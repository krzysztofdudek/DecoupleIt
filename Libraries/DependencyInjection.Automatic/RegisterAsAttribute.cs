using System;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;

namespace GS.DecoupleIt.DependencyInjection.Automatic
{
    /// <summary>
    ///     Attribute's purpose is to indicate that class should be registered as given service type by
    ///     <see cref="ServiceCollectionExtensions.ScanAssemblyForImplementations" />.
    ///     It can be usable when we want to register class as its base type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    [MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class RegisterAsAttribute : Attribute
    {
        /// <summary>
        ///     Service type.
        /// </summary>
        [NotNull]
        public Type ServiceType { get; }

        /// <summary>
        ///     Creates an instance of <see cref="RegisterAsAttribute" />.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        public RegisterAsAttribute([NotNull] Type serviceType)
        {
            ContractGuard.IfArgumentIsNull(nameof(serviceType), serviceType);

            ServiceType = serviceType;
        }
    }
}
