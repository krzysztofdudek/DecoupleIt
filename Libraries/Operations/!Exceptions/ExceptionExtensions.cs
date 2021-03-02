using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations
{
    /// <summary>
    ///     Extends <see cref="Exception" />.
    /// </summary>
    [PublicAPI]
    public static class ExceptionExtensions
    {
        /// <summary>
        ///     Sets category.
        /// </summary>
        /// <param name="exception">Exception.</param>
        /// <param name="category">Category.</param>
        /// <typeparam name="TException">Exception type.</typeparam>
        /// <returns>This exception.</returns>
        [NotNull]
        public static TException WithCategory<TException>([NotNull] this TException exception, [NotNull] string category)
            where TException : Exception
        {
            return exception.WithData("Category", category);
        }

        /// <summary>
        ///     Adds or overrides key in data property.
        /// </summary>
        /// <param name="exception">Exception.</param>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <typeparam name="TException">Exception type.</typeparam>
        /// <returns>This exception.</returns>
        [NotNull]
        public static TException WithData<TException>([NotNull] this TException exception, [NotNull] object key, [NotNull] object value)
            where TException : Exception
        {
            if (exception.Data.Contains(key))
                exception.Data[key] = value;
            else
                exception.Data.Add(key, value);

            return exception;
        }
    }
}
