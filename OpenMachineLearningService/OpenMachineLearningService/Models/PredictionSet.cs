using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenMachineLearningService.Models
{
    /// <summary>
    /// Defines the predictions for unspecified input.
    /// </summary>
    public class PredictionSet
    {
        /// <summary>
        /// The predictions for unspecified input.
        /// </summary>
        public List<Prediction> Predictions { get; set; }

    }
}