using System;

namespace GS.DecoupleIt.DependencyInjection.Automatic
{
    /// <summary>
    ///     Exception thrown when a type has ambiguous lifetime attributes assignment.
    /// </summary>
    public sealed class LifeTimeAttributeAmbiguity : Exception
    {
        /// <summary>
        ///     Creates an instance of <see cref="LifeTimeAttributeAmbiguity" />.
        /// </summary>
        /// <param name="message">Message.</param>
        public LifeTimeAttributeAmbiguity(string message) : base(message) { }
    }
}
