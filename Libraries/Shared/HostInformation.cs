using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Shared
{
    public sealed class HostInformation : IHostInformation
    {
        public string Environment { get; }

        public Guid Identifier { get; }

        public string Name { get; }

        public string Version { get; }

        public HostInformation(
            Guid identifier,
            [NotNull] string name,
            [CanBeNull] string version,
            [CanBeNull] string environment)
        {
            Identifier  = identifier;
            Name        = name;
            Version     = version;
            Environment = environment;

            _enumerable = new KeyValuePair<string, object>[]
            {
                new("HostName", name),
                new("HostVersion", version),
                new("HostIdentifier", identifier)
            };
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _enumerable.GetEnumerator();
        }

        [NotNull]
        private readonly IEnumerable<KeyValuePair<string, object>> _enumerable;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
