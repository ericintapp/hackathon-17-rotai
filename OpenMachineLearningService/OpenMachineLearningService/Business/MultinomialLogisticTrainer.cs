using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenMachineLearningService.Business
{
    using System.Data;

    using Accord.MachineLearning.Bayes;
    using Accord.Math;
    using Accord.Statistics.Distributions.Fitting;
    using Accord.Statistics.Distributions.Univariate;
    using Accord.Statistics.Filters;
    using Accord.Statistics.Models.Regression;
    using Accord.Statistics.Models.Regression.Fitting;

    public class MultinomialLogisticTrainer : ITrainer<TrainerHelper>
    {
        public TrainerHelper Train(System.Data.DataTable table, string columnName)
        {
            var container = new TrainerHelper();
            var trainingCodification = new Codification() { DefaultMissingValueReplacement = Double.NaN };
            trainingCodification.Learn(table);
            DataTable symbols = trainingCodification.Apply(table);
            container.columnNamesArray =
                table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).Where(s => s != columnName).ToArray();


            double[][] inputs = symbols.ToJagged(container.columnNamesArray);
            int[] outputs = symbols.ToArray<int>(columnName);

            
            var teacher = new NaiveBayesLearning<NormalDistribution>();

            // Set options for the component distributions
            teacher.Options.InnerOption = new NormalOptions
            {
                Regularization = 1e-5 // to avoid zero variances
            };

            if (inputs.Length > 0)
            {
                NaiveBayes<NormalDistribution> learner = teacher.Learn(inputs, outputs);
                container.trainer = learner;
            }

            //var lbnr = new LowerBoundNewtonRaphson() { MaxIterations = 100, Tolerance = 1e-6 };
            //var mlr = lbnr.Learn(inputs, outputs);
            container.codification = trainingCodification;
            container.symbols = symbols;
            return container;
        }

        public KeyValuePair<string, Double> Decide(TrainerHelper container, string[] inputs, string columnName)
        {
            if (container.trainer == null)
            {
                return new KeyValuePair<string, Double>(null, 0.0);
            }

            string[] testInputNames = container.columnNamesArray;
            List<double> inputsList = new List<double>();
            int i = 0;
            const double unspecified = -1.0;
            foreach (string input in inputs)
            {
                if (string.IsNullOrEmpty(input))
                {
                    inputsList.Add(unspecified);
                }
                else
                {
                    try
                    {
                        inputsList.Add(container.codification.Transform(testInputNames[i], input));

                    }
                    catch
                    {
                        inputsList.Add(unspecified);
                        
                    }
                }
                i++;
            }

            double[] testInputs = inputsList.ToArray<double>();
            int predicted = container.trainer.Decide(testInputs);
            string predictedValue = container.codification.Revert(columnName, predicted);
            var confidences = container.trainer.Probabilities(testInputs);
            var confidence = 0.0;
            if (confidences.Length > 0)
            {
                confidence = confidences[0];
            }
            KeyValuePair<string, Double> keyValuePair = new KeyValuePair<string, Double>(predictedValue, confidence);
            return keyValuePair;
        }
    }
}