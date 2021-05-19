using System.Collections.Generic;
using GS.DecoupleIt.Options.Automatic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Contextual.UnitOfWork
{
    /// <summary>
    ///     Contextual unit of work options.
    /// </summary>
    [ConfigureAsNamespace]
    [PublicAPI]
    public sealed class Options
    {
        /// <summary>
        ///     Pooling options.
        /// </summary>
        [NotNull]
        public PoolingOptions Pooling { get; set; } = new();

        /// <summary>
        ///     Pooling options.
        /// </summary>
        [PublicAPI]
        public sealed class PoolingOptions
        {
            /// <summary>
            ///     Default options.
            /// </summary>
            [NotNull]
            public UnitOfWorkPoolingOptions Default { get; set; } = new()
            {
                MaxPoolSize = 20
            };

            /// <summary>
            ///     Is pooling enabled.
            /// </summary>
            public bool Enabled { get; set; } = true;

            /// <summary>
            ///     Options dedicated for specific unit of works.
            /// </summary>
            [NotNull]
            public Dictionary<string, UnitOfWorkPoolingOptions> UnitOfWorks { get; set; } = new();
        }

        /// <summary>
        ///     Unit of work pooling options.
        /// </summary>
        [PublicAPI]
        public sealed class UnitOfWorkPoolingOptions
        {
            public int MaxPoolSize { get; set; }
        }
    }
}
