using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenMachineLearningService.Business
{
    using System.Data;

    using Accord.MachineLearning;
    using Accord.MachineLearning.Bayes;
    using Accord.MachineLearning.DecisionTrees;
    using Accord.Statistics.Distributions.Univariate;
    using Accord.Statistics.Filters;
    using Accord.Statistics.Models.Regression;

    public class TrainerHelper
    {
        public Codification codification;
        public NaiveBayes<NormalDistribution> trainer;
        public string[] columnNamesArray;
        public DataTable symbols;

        public DecisionTree decisionTree;
    }
}