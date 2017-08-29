// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScenarioManager.cs" company="">
//   
// </copyright>
// <summary>
//   The scenario manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace OpenMachineLearningService.Business
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data;
    using System.IO;
    using System.Linq;

    using Accord.IO;

    using Microsoft.Ajax.Utilities;

    using OpenMachineLearningService.Models;

    /// <summary>
    ///     The scenario manager.
    /// </summary>
    public class ScenarioManager
    {
        #region Static Fields

        /// <summary>
        /// The training by scenario.
        /// </summary>
        private static readonly Dictionary<string, ScenarioTrainings> trainingByScenario =
            new Dictionary<string, ScenarioTrainings>();

        #endregion

        #region Public Methods and Operators

        public void CompleteInputSet(string scenarioId, string inputSetId)
        {
            using (var dbContext = new OpenAIEntities1())
            {
                Scenario entity = dbContext.Scenarios.Find(scenarioId);
                if (entity == null)
                {
                    throw new Exception(string.Format("Unable to find scenario for {0}", scenarioId));
                }
                
                List<Feature> features =
                    dbContext.Features.Where(f => f.ScenarioId == scenarioId).OrderBy(f => f.Position).ToList();
                Dictionary<string, Input> inputsById =
                    dbContext.Inputs.Where(i => i.ScenarioId == scenarioId && i.InputSetId == inputSetId)
                        .ToDictionary(i => i.FeatureId);

                var sortedInputs = new List<Input>();
                foreach (Feature feature in features)
                {
                    Input input;
                    if (inputsById.TryGetValue(feature.FeatureId, out input))
                    {
                        sortedInputs.Add(input);
                    }
                    else
                    {
                        sortedInputs.Add(new Input { FeatureId = feature.FeatureId });
                    }
                }

                var newData = string.Join(",", sortedInputs.Select(i => i.Value ?? "0"));
                entity.Contents = entity.Contents + Environment.NewLine + newData;
                dbContext.SaveChanges();
                this.Train(scenarioId);
            }

        }

        /// <summary>
        /// The create inputs.
        /// </summary>
        /// <param name="scenarioId">
        /// The scenario id.
        /// </param>
        /// <param name="inputSetId">
        /// The input set id.
        /// </param>
        /// <param name="inputs">
        /// The inputs.
        /// </param>
        public PredictionSet CreateInputs(string scenarioId, string inputSetId, List<Models.Input> inputs)
        {
            if (inputs == null)
            {
                return new PredictionSet();
            }

            using (var dbContext = new OpenAIEntities1())
            {
                InputSet inputSet =
                    dbContext.InputSets.FirstOrDefault(i => i.ScenarioId == scenarioId && i.InputSetId == inputSetId);
                if (inputSet == null)
                {
                    inputSet = new InputSet();
                    inputSet.ScenarioId = scenarioId;
                    inputSet.InputSetId = inputSetId;
                    dbContext.InputSets.Add(inputSet);
                }

                Dictionary<string, Input> existing =
                    dbContext.Inputs.Where(i => i.ScenarioId == scenarioId && i.InputSetId == inputSetId)
                        .ToDictionary(i => i.FeatureId);

                foreach (Models.Input input in inputs)
                {
                    Input entity;
                    existing.TryGetValue(input.InputId, out entity);
                    if (entity == null)
                    {
                        entity = new Input();
                        dbContext.Inputs.Add(entity);
                        entity.ScenarioId = scenarioId;
                        entity.InputSetId = inputSetId;
                        entity.FeatureId = input.InputId;
                    }

                    entity.Value = input.Value;
                }

                dbContext.SaveChanges();
            }

            return this.Predict(scenarioId, inputSetId);
        }

        /// <summary>
        /// The create scenario.
        /// </summary>
        /// <param name="scenario">
        /// The scenario.
        /// </param>
        public void CreateScenario(Models.Scenario scenario)
        {
            using (var dbContext = new OpenAIEntities1())
            {
                Scenario entity = dbContext.Scenarios.Find(scenario.Id);
                if (entity == null)
                {
                    entity = new Scenario();
                    entity.ScenarioId = scenario.Id;
                    dbContext.Scenarios.Add(entity);
                }

                entity.Name = scenario.Name;
                entity.Contents = scenario.Contents;

                // Add the features
                DataTable table = this.ParseCsv(scenario.Contents);
                int position = 1;
                foreach (DataColumn column in table.Columns)
                {
                    if (entity.Features.Any(f => f.FeatureId == column.ColumnName))
                    {
                        continue;
                    }

                    var feature = new Feature();
                    feature.FeatureId = column.ColumnName;
                    feature.ScenarioId = scenario.Id;
                    feature.Position = position++;
                    entity.Features.Add(feature);
                }

                dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// The parse csv.
        /// </summary>
        /// <param name="csvContents">
        /// The csv contents.
        /// </param>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        public DataTable ParseCsv(string csvContents)
        {
            using (var sr = new StringReader(csvContents))
            {
                var reader = new CsvReader(sr, true);
                return reader.ToTable();
            }
        }

        private ITrainer<TrainerHelper> trainer = new MultinomialLogisticTrainer();
 
        /// <summary>
        /// The predict.
        /// </summary>
        /// <param name="scenarioId">
        /// The scenario id.
        /// </param>
        /// <param name="inputSetId">
        /// The input set id.
        /// </param>
        /// <returns>
        /// The <see cref="PredictionSet"/>.
        /// </returns>
        public PredictionSet Predict(string scenarioId, string inputSetId)
        {
            var predictions = new PredictionSet { Predictions = new List<Prediction>() };
            ScenarioTrainings trainings;
            if (!trainingByScenario.TryGetValue(scenarioId, out trainings))
            {
                trainings = this.Train(scenarioId);
            }

            if (trainings == null)
            {
                return predictions;
            }

            using (var dbContext = new OpenAIEntities1())
            {
                List<Feature> features =
                    dbContext.Features.Where(f => f.ScenarioId == scenarioId).OrderBy(f => f.Position).ToList();
                Dictionary<string, Input> inputsById =
                    dbContext.Inputs.Where(i => i.ScenarioId == scenarioId && i.InputSetId == inputSetId)
                        .ToDictionary(i => i.FeatureId);

                var sortedInputs = new List<Input>();
                foreach (Feature feature in features)
                {
                    Input input;
                    if (inputsById.TryGetValue(feature.FeatureId, out input))
                    {
                        sortedInputs.Add(input);
                    }
                    else
                    {
                        sortedInputs.Add(new Input { FeatureId = feature.FeatureId });
                    }
                }


                List<string> unset =
                    features.Select(f => f.FeatureId).Except(inputsById.Values.Select(i => i.FeatureId)).ToList();
                foreach (string inputId in unset)
                {
                    TrainerHelper training;
                    if (!trainings.TrainingByFeatureId.TryGetValue(inputId, out training))
                    {
                        continue;
                    }

                    var inputsMinusFeature = sortedInputs.Where(i => i.FeatureId != inputId).Select(i => i.Value).ToArray();
                    KeyValuePair<string, double> valueAndConfidence = trainer.Decide(training, inputsMinusFeature, inputId);

                    var prediction = new Prediction
                                         {
                                             InputId = inputId, 
                                             Confidence = valueAndConfidence.Value, 
                                             Value = valueAndConfidence.Key
                                         };
                    predictions.Predictions.Add(prediction);
                }
            }

            return predictions;
        }

        /// <summary>
        /// The train.
        /// </summary>
        /// <param name="scenarioId">
        /// The scenario id.
        /// </param>
        /// <returns>
        /// The <see cref="ScenarioTrainings"/>.
        /// </returns>
        public ScenarioTrainings Train(string scenarioId)
        {
            using (var dbContext = new OpenAIEntities1())
            {
                Scenario scenario = dbContext.Scenarios.Find(scenarioId);
                if (scenario == null)
                {
                    return null;
                }

                DataTable table = this.ParseCsv(scenario.Contents);

                // Get hypothesis for each feature / output
                var trainings = new ScenarioTrainings
                                    {
                                        ScenarioId = scenarioId, 
                                        TrainingByFeatureId = new Dictionary<string, TrainerHelper>()
                                    };

                trainingByScenario[scenarioId] = trainings;
                foreach (DataColumn column in table.Columns)
                {
                    TrainerHelper thing = trainer.Train(table, column.ColumnName);
                    trainings.TrainingByFeatureId[column.ColumnName] = thing;
                }

                return trainings;
            }
        }

        /// <summary>
        /// Tests the prediction using the current algorithm on the supplied data.
        /// </summary>
        /// <param name="scenarioId">
        /// The scenario id.
        /// </param>
        /// <returns>
        /// The predictions for the specified feature ID.
        /// </returns>
        public TestPredictions Test(string scenarioId, string featureId, Contents contents)
        {
            using (var dbContext = new OpenAIEntities1())
            {
                var scenario = dbContext.Scenarios.Find(scenarioId);
                if (scenario == null)
                {
                    return null;
                }

                ScenarioTrainings trainings;
                if (!trainingByScenario.TryGetValue(scenarioId, out trainings))
                {
                    trainings = this.Train(scenarioId);
                }

                var table = this.ParseCsv(contents.Data);

                TrainerHelper training;
                if (!trainings.TrainingByFeatureId.TryGetValue(featureId, out training))
                {
                    return null;
                }

                var predictions = trainer.Decide(training, table, featureId);

                return predictions;
            }
        }

        #endregion
    }
}