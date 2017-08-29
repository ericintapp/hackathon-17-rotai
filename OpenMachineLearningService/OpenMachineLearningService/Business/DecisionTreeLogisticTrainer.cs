using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenMachineLearningService.Business
{
    using System.Data;
    using System.Text;

    using Accord.MachineLearning.Bayes;
    using Accord.MachineLearning.DecisionTrees;
    using Accord.MachineLearning.DecisionTrees.Learning;
    using Accord.Math;
    using Accord.Statistics.Distributions.Fitting;
    using Accord.Statistics.Distributions.Univariate;
    using Accord.Statistics.Filters;
    using Accord.Statistics.Models.Regression;
    using Accord.Statistics.Models.Regression.Fitting;

    using OpenMachineLearningService.Models;

    public class DecisionTreeLogisticTrainer : ITrainer<TrainerHelper>
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
            int[][] tempInputs = symbols.ToJagged<int>(container.columnNamesArray);
            double[][] inputs = new double[tempInputs.Length][];
            for (var i = 0; i < tempInputs.Length; i++)
            {
               // var flattened = this.ExpandRow(trainingCodification, tempInputs[i], columnOrdinal);
               // inputs[i] = flattened;
            }


            int[] outputs = symbols.ToArray<int>(columnName);

            var id3learning = new ID3Learning();

            id3learning.Attributes = DecisionVariable.FromCodebook(trainingCodification);
            // Learn the training instances!
            DecisionTree tree = id3learning.Learn(tempInputs, outputs);
            container.decisionTree = tree;
           

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
            int predicted = container.decisionTree.Decide(testInputs);
            string predictedValue = container.codification.Revert(columnName, predicted);
            //var confidences = container.decisionTree. (testInputs);
            var confidence = 0.0;
            //if (confidences.Length > 0)
            {
                //confidence = confidences[0];
            }
            KeyValuePair<string, Double> keyValuePair = new KeyValuePair<string, Double>(predictedValue, confidence * 100);
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
            for (var i = 0; i < tempInputs.Length; i++)
            {
                var flattened = this.ExpandRow(container.codification, tempInputs[i], columnOrdinal);
                inputs[i] = flattened;
            }



            int[] outputs = symbols.ToArray<int>(inputId);



            var foo = new TestPredictions();
            var predictions = new List<KeyValuePair<string, double>>();
            //foo.Predictions = predictions;
            var predicted = container.decisionTree.Decide(inputs);
            var predictedValues = container.codification.Revert(inputId, predicted);
            //var confidences = container.trainer.Probabilities(inputs);
            var correct = 0;
            var incorrect = 0;
            var contents = new StringBuilder();
            for (var i = 0; i < predicted.Length; i++)
            {
                var predictedAsInt = predicted[i];
                var actualValue = outputs[i];

                var predictedValue = predictedValues[i];
                //var c = confidences[i];
                var p = 0.0;
                //if (c.Length > 0)
                {
                    //p = c[0];
                }

                p = p * 100;
                if (predictedAsInt == actualValue)
                {
                    correct++;
                }
                else
                {
                    if (p > 75)
                    {
                        incorrect++;
                    }
                }

                var a = new KeyValuePair<string, Double>(predictedValue, p);
                predictions.Add(a);

                contents.Append(string.Format("{0},{1},{2}" + Environment.NewLine, actualValue, predictedValue, p));

            }

            foo.Total = predicted.Length;
            foo.Correct = correct;
            foo.Incorrect = incorrect;
            foo.Contents = contents.ToString();
            return foo;
        }


    }
}