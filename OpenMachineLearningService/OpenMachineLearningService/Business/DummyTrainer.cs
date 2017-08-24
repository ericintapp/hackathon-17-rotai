using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenMachineLearningService.Business
{
    public class DummyTrainer : ITrainer<String>
    {
        public string Train(System.Data.DataTable table, string columnName)
        {
            return string.Empty;
        }

        public KeyValuePair<string, double> Decide(string trainer, string[] inputs, string columnName)
        {
            return new KeyValuePair<string, double>("Test", new Random().Next(0, 100));
        }
    }
}