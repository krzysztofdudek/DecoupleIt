namespace GS.DecoupleIt.Options.Automatic
{
    /// <summary>
    ///     Base interface for configuration attributes.
    /// </summary>
    public interface IConfigureAttribute
    {
        /// <summary>
        ///     Priority of loading configuration for many attributes.
        /// </summary>
        short Priority { get; }
    }
}
