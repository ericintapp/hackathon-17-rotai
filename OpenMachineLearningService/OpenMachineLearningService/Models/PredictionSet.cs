using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenMachineLearningService.Models
{
    public class PredictionSet
    {
        public KeyValuePair<string, Prediction>[] Predictions { get; set; }

    }
}