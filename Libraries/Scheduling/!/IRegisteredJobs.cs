using System.Collections.Generic;

namespace GS.DecoupleIt.Scheduling
{
    /// <summary>
    ///     Gathers registered jobs to be run later.
    /// </summary>
    public interface IRegisteredJobs : IEnumerable<JobEntry> { }
}
