using GS.DecoupleIt.Options.Automatic;

namespace GS.DecoupleIt.AspNetCore.Service
{
    /// <summary>
    ///     Options of a service.
    /// </summary>
    [ConfigureAsNamespace]
    public class ServiceOptions
    {
        /// <summary>
        ///     If request should be logged.
        /// </summary>
        public bool LogRequests { get; set; } = true;

        /// <summary>
        ///     If responses should be logged.
        /// </summary>
        public bool LogResponses { get; set; } = true;
    }
}
