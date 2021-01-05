using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations
{
    /// <summary>
    ///     Base class of extended exception. Allows to define category of an exception, which can provide difference in logging levels.
    /// </summary>
    [PublicAPI]
    public abstract class ExtendedException : Exception
    {
        /// <summary>
        ///     Category.
        /// </summary>
        [NotNull]
        public string Category { get; }

        /// <inheritdoc />
        /// <param name="category">Category.</param>
        protected ExtendedException([NotNull] string category)
        {
            Category = category;
        }

        /// <inheritdoc />
        /// <param name="category">Category.</param>
        protected ExtendedException([NotNull] string category, [NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Category = category;
        }

        /// <inheritdoc />
        /// <param name="category">Category.</param>
        protected ExtendedException([NotNull] string category, string message) : base(message)
        {
            Category = category;
        }

        /// <inheritdoc />
        /// <param name="category">Category.</param>
        protected ExtendedException([NotNull] string category, string message, Exception innerException) : base(message, innerException)
        {
            Category = category;
        }
    }
}
