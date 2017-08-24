using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenMachineLearningService.Business
{
    using System.Data;

    using Accord.IO;

    using OpenMachineLearningService.Models;

    public class ScenarioManager
    {
        public void CreateScenario(Models.Scenario scenario)
        {
            using (var dbContext = new OpenAIEntities())
            {
                var entity = dbContext.Scenarios.Find(scenario.Id);
                if (entity == null)
                {
                    entity = new Business.Scenario();
                }
                entity.Id = scenario.Id;
                entity.Name = scenario.Name;
                entity.Contents = scenario.Contents;
                dbContext.Scenarios.Add(entity);

                dbContext.SaveChanges();
            }
        }

        public DataTable ParseCsv(string csvContents)
        {
            var reader = new CsvReader(csvContents, true);
            return reader.ToTable();
        }
    }
}