namespace OpenMachineLearningService.Models
{
    using System.Collections.Generic;

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