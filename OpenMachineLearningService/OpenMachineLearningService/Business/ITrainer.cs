using System;
using System.Collections.Generic;

namespace OpenMachineLearningService.Business
{
    using System.Data;

    using OpenMachineLearningService.Models;

    /// <summary>
    /// Interface for training and predicting data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITrainer<T>
    {
        /// <summary>
        /// Trains the data to solve for the specified feature ID.
        /// </summary>
        /// <param name="table">The table</param>
        /// <param name="featureId">The feature ID</param>
        /// <returns>The trainer (used to predict)</returns>
        T Train(DataTable table, string featureId);

        /// <summary>
        /// Given the inputs, predicts for the feature ID.
        /// </summary>
        /// <param name="trainer">The trainer</param>
        /// <param name="inputs">The inputs</param>
        /// <param name="featureId">The output feature</param>
        /// <returns>The predicted result and confidence</returns>
        KeyValuePair<string, Double> Decide(T trainer, string[] inputs, string featureId);


        TestPredictions Decide(TrainerHelper container, DataTable table, string inputId);

    }
}