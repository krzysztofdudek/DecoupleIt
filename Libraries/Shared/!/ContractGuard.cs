using System;
using System.Collections;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Shared
{
    /// <summary>
    ///     Provides methods simplifying validation of public contracts.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Global")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
    [PublicAPI]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal static class ContractGuard
    {
        /// <summary>
        ///     Validates if <paramref name="enumerable" /> contains <see langword="null" /> items.
        /// </summary>
        /// <param name="argumentName">Argument name.</param>
        /// <param name="enumerable">Enumerable.</param>
        /// <exception cref="ArgumentException">Argument "<paramref name="argumentName" />" contains <see langword="null" /> items.</exception>
        public static void IfArgumentContainsNullItems([NotNull] string argumentName, [NotNull] [InstantHandle] IEnumerable enumerable)
        {
            RequiresArgumentName(argumentName);

            var enumerator = enumerable.GetEnumerator();

            while (enumerator.MoveNext())
                if (enumerator.Current == null)
                    throw new ArgumentException($"Argument \"{argumentName}\" contains null items.");
        }

        /// <summary>
        ///     Throws <see cref="ArgumentNullException" /> if <paramref name="object" /> is <see langword="null" />.
        /// </summary>
        /// <param name="argumentName">Argument name.</param>
        /// <param name="object">Object.</param>
        /// <exception cref="ArgumentNullException">Argument "<paramref name="argumentName" />" is <see langword="null" />.</exception>
        [AssertionMethod]
        [ContractAnnotation("object:null=>halt")]
        public static void IfArgumentIsNull(
            [NotNull] string argumentName,
            [CanBeNull] [AssertionCondition(AssertionConditionType.IS_NOT_NULL)]
            object @object)
        {
            RequiresArgumentName(argumentName);

            if (@object is null)
                throw new ArgumentNullException(argumentName, $"Argument \"{argumentName}\" is null.");
        }

        /// <summary>
        ///     Throws <see cref="ArgumentException" /> if <paramref name="string" /> is <see langword="null" /> or whitespace.
        /// </summary>
        /// <param name="argumentName">Argument name.</param>
        /// <param name="string">String.</param>
        /// <exception cref="ArgumentNullException">
        ///     Argument "<paramref name="argumentName" />" is <see langword="null" /> or
        ///     whitespace.
        /// </exception>
        [AssertionMethod]
        [ContractAnnotation("string:null=>halt")]
        public static void IfArgumentIsNullOrWhitespace(
            [NotNull] string argumentName,
            [CanBeNull] [AssertionCondition(AssertionConditionType.IS_NOT_NULL)]
            string @string)
        {
            RequiresArgumentName(argumentName);

            if (string.IsNullOrWhiteSpace(@string))
                throw new ArgumentException($"Argument \"{argumentName}\" is null or whitespace.", argumentName);
        }

        /// <summary>
        ///     Validates if <paramref name="enumerable" /> is not null and does not contains <see langword="null" /> items.
        /// </summary>
        /// <param name="argumentName">Argument name.</param>
        /// <param name="enumerable">Enumerable.</param>
        /// <exception cref="ArgumentNullException">Argument "<paramref name="argumentName" />" is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">Argument "<paramref name="argumentName" />" contains <see langword="null" /> items.</exception>
        [AssertionMethod]
        [ContractAnnotation("enumerable:null=>halt")]
        public static void IfArgumentNullOrContainsNullItems(
            [NotNull] string argumentName,
            [CanBeNull] [ItemCanBeNull] [AssertionCondition(AssertionConditionType.IS_NOT_NULL)]
            IEnumerable enumerable)
        {
            IfArgumentIsNull(argumentName, enumerable);
            IfArgumentContainsNullItems(argumentName, enumerable);
        }

        /// <summary>
        ///     Validates if <paramref name="enum" /> has value out of range of defining enum.
        /// </summary>
        /// <param name="argumentName">Argument name.</param>
        /// <param name="enum">Enum value.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Argument "<paramref name="argumentName" />" is out of range of defining
        ///     enum.
        /// </exception>
        public static void IfEnumArgumentIsOutOfRange([NotNull] string argumentName, [NotNull] Enum @enum)
        {
            RequiresArgumentName(argumentName);

            if (!Enum.IsDefined(@enum.GetType(), @enum))
                throw new ArgumentOutOfRangeException(argumentName, $"Argument \"{argumentName}\" is out of range of defining enum.");
        }

        private static void RequiresArgumentName([NotNull] string argumentName)
        {
            if (string.IsNullOrWhiteSpace(argumentName))
                throw new ArgumentNullException(nameof(argumentName));
        }
    }
}
