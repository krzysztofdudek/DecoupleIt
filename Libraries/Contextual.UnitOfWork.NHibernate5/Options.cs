using GS.DecoupleIt.Options.Automatic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Contextual.UnitOfWork.NHibernate5
{
    /// <summary>
    ///     Allows to configure default behaviour of NHibernate extension.
    /// </summary>
    [ConfigureAsNamespace]
    public sealed class Options
    {
        /// <summary>
        ///     Allows to configure transaction behaviour.
        /// </summary>
        [NotNull]
        public TransactionOptions Transaction { get; set; } = new();
    }
}
