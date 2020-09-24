using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Persistence.Automatic
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature, ImplicitUseTargetFlags.WithMembers)]
    public sealed class PersistAttribute : Attribute
    {
        public PersistAttribute([CanBeNull] string contextName = default)
        {
            ContextName = contextName;
        }

        [CanBeNull]
        internal string ContextName { get; }
    }
}
