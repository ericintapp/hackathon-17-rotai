using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenMachineLearningService.Controllers
{
    using System.Web.Http;

    using OpenMachineLearningService.Business;
    using OpenMachineLearningService.Models;

    using Input = OpenMachineLearningService.Models.Input;
    using Scenario = OpenMachineLearningService.Models.Scenario;

    [RoutePrefix("api/v1")]
    public class OpenAIController : ApiController
    {
        /// <summary>
        /// Creates the scenario.
        /// </summary>
        /// <param name="scenarioId"></param>
        /// <param name="csvContents">Contains content for csv delimited data in which first row contains input Ids</param>
        [HttpPost]
        [Route("scenario")]
        public void CreateScenario(Scenario scenario)
        {
            new ScenarioManager().CreateScenario(scenario);
        }

        [Route("scenario/{scenarioId}/{inputSetId}/input")]
        [HttpPost]
        public void AddScenarioInput(string scenarioId, string inputSetId, Input input)
        {

        }

        [HttpPost]
        [Route("scenario/{scenarioId}/{inputSetId}/inputs")]
        public void AddScenarioInputs(string scenarioId, string inputSetId, Input[] inputs)
        {

        }

        [HttpPost]
        [Route("scenario/{scenarioId}/{inputSetId}/_predict")]
        public PredictionSet Predict(string scenarioId, string inputSetId)
        {
            return new PredictionSet();
        }
    }
}