using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenMachineLearningService.Models
{
    public class TestPredictions
    {
        public int Correct { get; set; }

        public int Incorrect { get; set; }
        public int Total { get; set; }

        public string Contents { get; set; }

        public List<KeyValuePair<string, double>> Predictions { get; set; }
    }
}