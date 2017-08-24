using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenMachineLearningService.Business
{
    using System.Data;
    using System.Data.SqlTypes;

    using Accord.Statistics.Models.Regression;

    public interface ITrainer<T>
    {
        T Train(DataTable table, string columnName);

        KeyValuePair<string, Double> Decide(T trainer, string[] inputs, string columnName);
    }
}