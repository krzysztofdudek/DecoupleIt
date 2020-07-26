using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Shared
{
    /// <summary>
    ///     Extends <see cref="Type" />.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        ///     Gets value of constant defined within type.
        /// </summary>
        /// <param name="type">Owner type of constant.</param>
        /// <param name="name">Name of constant.</param>
        /// <typeparam name="TValue">Value type.</typeparam>
        /// <returns>Value</returns>
        public static TValue GetConstantOfName<TValue>([NotNull] this Type type, [NotNull] string name)
        {
            var field = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                            .SingleOrDefault(fi => !fi.IsInitOnly && fi.FieldType == typeof(TValue) && fi.Name == name);

            if (field != null)
                return (TValue) field.GetRawConstantValue();

            return default;
        }
    }
}
