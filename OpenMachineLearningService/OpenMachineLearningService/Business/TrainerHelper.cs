using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenMachineLearningService.Business
{
    using Accord.Statistics.Models.Regression;

    public class TrainerHelper
    {
        public MultinomialLogisticRegression MultinomialLogisticRegression { get; set; }
    }
}