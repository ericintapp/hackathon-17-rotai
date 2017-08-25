using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenMachineLearningService.Models
{
    /// <summary>
    /// Defines a prediction.
    /// </summary>
    public class Prediction
    {
        /// <summary>
        /// The input (feature) ID.
        /// </summary>
        public string InputId { get; set; }

        /// <summary>
        /// The predicted value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The confidence in the predicted value.
        /// </summary>
        public double Confidence { get; set; }
    }
}