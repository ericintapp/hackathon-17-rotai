using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenMachineLearningService.Business
{
    using System.Data;

    using Accord.Math;
    using Accord.Statistics.Filters;
    using Accord.Statistics.Models.Regression;
    using Accord.Statistics.Models.Regression.Fitting;

    public class MultinomialLogisticTrainer : ITrainer<TrainerHelper>
    {
        public TrainerHelper Train(System.Data.DataTable table, string columnName)
        {
            var trainingCodification = new Codification()
            {
                DefaultMissingValueReplacement = Double.NaN
            };
            trainingCodification.Learn(table);
            var symbols = trainingCodification.Apply(table);

            string[] columnNamesArray = table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).Where(s => s != columnName).ToArray();
            double[][] inputs = symbols.ToJagged(columnNamesArray);
            int[] outputs = symbols.ToArray<int>(columnName);
            var lbnr = new LowerBoundNewtonRaphson()
            {
                MaxIterations = 100,
                Tolerance = 1e-6
            };
            var mlr = lbnr.Learn(inputs, outputs);
            return new TrainerHelper { MultinomialLogisticRegression = mlr };
        }

        public KeyValuePair<string, double> Decide(TrainerHelper trainer, string[] inputs, string columnName)
        {
            return new KeyValuePair<string, double>("Test", new Random().Next(0, 100));
        }
    }
}