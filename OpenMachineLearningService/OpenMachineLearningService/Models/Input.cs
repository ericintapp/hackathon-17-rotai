using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenMachineLearningService.Models
{
    /// <summary>
    /// Defines an input.
    /// </summary>
    public class Input
    {
        /// <summary>
        /// The input (or feature) ID.
        /// </summary>
        public string InputId { get; set; }

        /// <summary>
        /// The value of the input.
        /// </summary>
        public string Value { get; set; }
    }
}