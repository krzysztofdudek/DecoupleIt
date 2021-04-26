using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Shared
{
    [PublicAPI]
    public interface IHostInformation : IEnumerable<KeyValuePair<string, object>>
    {
        [CanBeNull]
        public string Environment { get; }

        public Guid Identifier { get; }

        [NotNull]
        public string Name { get; }

        [CanBeNull]
        public string Version { get; }
    }
}
