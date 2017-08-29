namespace OpenMachineLearningService.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Web.Http;

    using OpenMachineLearningService.Business;
    using OpenMachineLearningService.Models;

    using Input = OpenMachineLearningService.Models.Input;
    using Scenario = OpenMachineLearningService.Models.Scenario;

    /// <summary>
    /// The open ai controller.
    /// </summary>
    [RoutePrefix("api/v1")]
    public class OpenAIController : ApiController
    {
        #region Public Methods and Operators

        /// <summary>
        /// Adds input data to an input set that is currently in progress.  Based on this new input as well as existing input,
        ///     a set of predictions for unspecified input is returned.
        /// </summary>
        /// <param name="scenarioId">
        /// The scenario ID
        /// </param>
        /// <param name="inputSetId">
        /// The input set ID
        /// </param>
        /// <param name="input">
        /// The new input
        /// </param>
        /// <returns>
        /// Predictions of unspecified data.
        /// </returns>
        [Route("scenario/{scenarioId}/{inputSetId}/input")]
        [HttpPost]
        public PredictionSet AddScenarioInput(string scenarioId, string inputSetId, Input input)
        {
            return new ScenarioManager().CreateInputs(scenarioId, inputSetId, new List<Input> { input });
        }

        /// <summary>
        /// Marks the input set as complete, and updates aglorithm with new input set.
        /// </summary>
        /// <param name="scenarioId">
        /// The scenario ID
        /// </param>
        /// <param name="inputSetId">
        /// The input set ID
        /// </param>
        [Route("scenario/{scenarioId}/{inputSetId}/_complete")]
        [HttpPost]
        public void FinishInputSet(string scenarioId, string inputSetId)
        {
            new ScenarioManager().CompleteInputSet(scenarioId, inputSetId);
        }

        /// <summary>
        /// Adds the list of input data to an input set that is currently in progress.  Based on this new input as well as
        ///     existing input,
        ///     a set of predictions for unspecified input is returned.
        /// </summary>
        /// <param name="scenarioId">
        /// The scenario ID.
        /// </param>
        /// <param name="inputSetId">
        /// The input set ID.
        /// </param>
        /// <param name="inputs">
        /// The inputs
        /// </param>
        /// <returns>
        /// Predictions of unspecified inputs.
        /// </returns>
        [HttpPost]
        [Route("scenario/{scenarioId}/{inputSetId}/inputs")]
        public PredictionSet AddScenarioInputs(string scenarioId, string inputSetId, Input[] inputs)
        {
            return new ScenarioManager().CreateInputs(scenarioId, inputSetId, inputs.ToList());
        }

        /// <summary>
        /// Creates or updates the scenario.  A scenario consists of the scenario ID, definition of the features and training
        ///     data.
        /// </summary>
        /// <param name="scenario">
        /// The scenario.
        /// </param>
        [HttpPost]
        [Route("scenario")]
        public void CreateScenario(Scenario scenario)
        {
            new ScenarioManager().CreateScenario(scenario);
        }

        /// <summary>
        /// Predicts output for any unspecified inputs.
        /// </summary>
        /// <param name="scenarioId">
        /// The scenario ID.
        /// </param>
        /// <param name="inputSetId">
        /// The input set ID.
        /// </param>
        /// <returns>
        /// Predictions of unspecified inputs.
        /// </returns>
        [HttpPost]
        [Route("scenario/{scenarioId}/{inputSetId}/_predict")]
        public PredictionSet Predict(string scenarioId, string inputSetId)
        {
            return new ScenarioManager().Predict(scenarioId, inputSetId);
        }

        /// <summary>
        /// Trains the scenario.  This is called implicitly when appropriate, but is provided here for demo purposes.
        /// </summary>
        /// <param name="scenarioId">
        /// The scenario ID.
        /// </param>
        [HttpPost]
        [Route("scenario/{scenarioId}/_train")]
        public void Train(string scenarioId)
        {
            new ScenarioManager().Train(scenarioId);
        }


        [HttpPost]
        [Route("scenario/{scenarioId}/{inputId}/_test")]
        public TestPredictions Test(string scenarioId, string inputId, Contents contents)
        {
            return new ScenarioManager().Test(scenarioId, inputId, contents);
        }

        #endregion
    }
}