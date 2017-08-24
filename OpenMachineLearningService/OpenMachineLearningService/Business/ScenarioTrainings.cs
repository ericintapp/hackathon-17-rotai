using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenMachineLearningService.Business
{
    using Accord.Statistics.Models.Regression;

    public class ScenarioTrainings
    {
        public string ScenarioId { get; set; }

        public Dictionary<string, String> TrainingByFeatureId { get; set; }
    }
}