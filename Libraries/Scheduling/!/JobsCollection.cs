using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Scheduling
{
    internal sealed class JobsCollection : IRegisteredJobs
    {
        public void AddRange([NotNull] IEnumerable<JobEntry> jobEntries)
        {
            var newJobs = jobEntries.Except(_jobs);

            _jobs.AddRange(newJobs);
        }

        public IEnumerator<JobEntry> GetEnumerator()
        {
            return _jobs.GetEnumerator();
        }

        [NotNull]
        private readonly List<JobEntry> _jobs = new List<JobEntry>();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
