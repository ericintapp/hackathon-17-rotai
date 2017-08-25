using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenMachineLearningService.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Scenario
    {
        /// <summary>
        /// The scenario ID.
        /// </summary>
        [Required]
        public string Id { get; set; }

        /// <summary>
        /// The scenario name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The contents of the scenario in csv delimited format.  The header row must specify the IDs of the features.
        /// </summary>
        [Required]
        public string Contents { get; set; }
    }
}