using JetBrains.Annotations;

namespace GS.DecoupleIt.Scheduling
{
    /// <summary>
    ///     Base interface dedicated for identification of schedule attributes.
    /// </summary>
    public interface IScheduleAttribute
    {
        /// <summary>
        ///     Validates configuration. It returns <see langword="null" /> if valid, description of an error if invalid.
        /// </summary>
        /// <returns><see langword="null" /> if valid, description of an error if invalid.</returns>
        [MustUseReturnValue]
        public string Validate();
    }
}
