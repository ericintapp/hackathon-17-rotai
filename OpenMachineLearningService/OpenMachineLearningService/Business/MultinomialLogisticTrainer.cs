﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenMachineLearningService.Business
{
    using System.Data;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.IO;
    using System.Text;

    using Accord.Extensions.Math.Geometry;
    using Accord.MachineLearning.Bayes;
    using Accord.Math;
    using Accord.Statistics.Distributions.Fitting;
    using Accord.Statistics.Distributions.Univariate;
    using Accord.Statistics.Filters;
    using Accord.Statistics.Kernels;
    using Accord.Statistics.Models.Regression;
    using Accord.Statistics.Models.Regression.Fitting;

    using OpenMachineLearningService.Models;

    public class MultinomialLogisticTrainer : ITrainer<TrainerHelper>
    {

        public double[] ExpandRow(Codification codification, double[] row, int columnToSkip)
        {
            return row;

            var flattenedRow = new List<double>();


            var choiceIndex = 0;
            for (var i = 0; i < row.Length; i++)
            {
                if (i == columnToSkip)
                {
                    choiceIndex = choiceIndex + 2;
                    continue;
                }

                var choices = codification[choiceIndex];
                var newOptions = new double[choices.Values.Length];
                int index = (int)row[i];
                newOptions[index] = 1;
                flattenedRow.AddRange(newOptions);
                choiceIndex++;
            }

            return flattenedRow.ToArray();
        }

        public TrainerHelper Train(System.Data.DataTable table, string columnName)
        {
            var container = new TrainerHelper();
            var trainingCodification = new Codification() { DefaultMissingValueReplacement = Double.NaN };
            trainingCodification.Learn(table);
            DataTable symbols = trainingCodification.Apply(table);
            container.columnNamesArray =
                table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).Where(s => s != columnName).ToArray();

            var columnOrdinal = table.Columns[columnName].Ordinal;
            double[][] tempInputs = symbols.ToJagged(container.columnNamesArray);
            double[][] inputs = new double[tempInputs.Length][];
            for (var i = 0; i < tempInputs.Length; i++)
            {
                var flattened = this.ExpandRow(trainingCodification, tempInputs[i], columnOrdinal);
                inputs[i] = flattened;
            }


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
            if (confidences.Length > 1)
            {
                confidence = confidences.Max();
            }
            KeyValuePair<string, Double> keyValuePair = new KeyValuePair<string, Double>(predictedValue, confidence*100);
            return keyValuePair;
        }


        public TestPredictions Decide(TrainerHelper container, DataTable table, string inputId)
        {
            if (container.trainer == null)
            {
                return null;
            }

            var symbols = container.codification.Apply(table);
            container.columnNamesArray =
                table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).Where(s => s != inputId).ToArray();

            var columnOrdinal = table.Columns[inputId].Ordinal;
            double[][] tempInputs = symbols.ToJagged(container.columnNamesArray);
            double[][] inputs = new double[tempInputs.Length][];







            var foo = new TestPredictions();
            var predictions = new List<KeyValuePair<string, double>>();
            var correct = 0;
            var incorrect = 0;
            var contents = new StringBuilder();
            foreach (DataRow x in table.Rows)
            {
                var values = x.ItemArray.Select(c => c.ToString()).ToList();
                var answer = values[columnOrdinal];
                values.RemoveAt(columnOrdinal);
                var prediction = this.Decide(container, values.ToArray(), inputId);

                contents.Append(string.Format("{0},{1},{2}" + Environment.NewLine, answer, prediction.Key, prediction.Value));
                if (answer == prediction.Key)
                {
                    correct++;
                }
                else
                {
                    if (prediction.Value > 75)
                    {
                        incorrect++;
                    }
                }

            }

            File.WriteAllText(@"c:\temp\hack1.txt", contents.ToString());

            int[] outputs = symbols.ToArray<int>(inputId);

            foo.Total = table.Rows.Count;
            foo.Correct = correct;
            foo.Incorrect = incorrect;
            foo.Contents = contents.ToString();
            return foo;
        }

    
    }
}