using System;
using GS.DecoupleIt.DependencyInjection.Automatic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Migrations
{
    /// <summary>
    ///     Attribute describes migration. Class is not inheritable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [BaseTypeRequired(typeof(IMigration))]
    public sealed class MigrationAttribute : SingletonAttribute
    {
        /// <summary>
        ///     Description.
        /// </summary>
        [NotNull]
        public string Description { get; }

        /// <summary>
        ///     Number.
        /// </summary>
        public long Number { get; }

        /// <summary>
        ///     Attribute describes migration.
        /// </summary>
        /// <param name="number">Number decides in what order migration should be executed.</param>
        /// <param name="description">Description gives a peek of actions performed during migration.</param>
        public MigrationAttribute(long number, [NotNull] string description)
        {
            Number      = number;
            Description = description;
        }
    }
}
