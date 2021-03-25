using GS.DecoupleIt.Options.Automatic;

namespace GS.DecoupleIt.DependencyInjection.Automatic
{
    [ConfigureAsNamespace]
    public sealed class Options
    {
        /// <summary>
        ///     Environment that is bound to the current running instance. It's used to register proper services destined for it.
        /// </summary>
        public string Environment { get; set; }
    }
}
