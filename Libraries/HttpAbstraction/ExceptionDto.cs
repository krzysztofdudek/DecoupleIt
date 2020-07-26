using System;

namespace GS.DecoupleIt.HttpAbstraction
{
    /// <summary>
    ///     Dto describing exception.
    /// </summary>
    public sealed class ExceptionDto
    {
        /// <summary>
        ///     Assembly containing exception.
        /// </summary>
        public string Assembly { get; set; }

        /// <summary>
        ///     Exception object.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        ///     Type of an exception.
        /// </summary>
        public string Type { get; set; }
    }
}
