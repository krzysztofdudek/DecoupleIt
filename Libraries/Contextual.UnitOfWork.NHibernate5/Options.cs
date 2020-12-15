using GS.DecoupleIt.Options.Automatic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Contextual.UnitOfWork.NHibernate5
{
    [ConfigureAsNamespace]
    public sealed class Options
    {
        [NotNull]
        public TransactionOptions Transaction { get; set; } = new TransactionOptions();
    }
}
