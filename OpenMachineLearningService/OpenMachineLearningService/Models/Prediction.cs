using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenMachineLearningService.Models
{
    public class Prediction
    {
        public string InputId { get; set; }

        public string Value { get; set; }

        public double Confidence { get; set; }
    }
}