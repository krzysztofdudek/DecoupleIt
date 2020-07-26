using GS.DecoupleIt.Shared;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Tracing
{
    /// <summary>
    ///     Metric.
    /// </summary>
    [PublicAPI]
    public readonly struct Metric
    {
        /// <summary>
        ///     Creates an instance of <see cref="Metric" />.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        public Metric([NotNull] string name, decimal value)
        {
            ContractGuard.IfArgumentIsNull(nameof(name), name);

            Name  = name;
            Value = value;
        }

        /// <summary>
        ///     Name.
        /// </summary>
        [NotNull]
        public string Name { get; }

        /// <summary>
        ///     Value.
        /// </summary>
        public decimal Value { get; }
    }
}
