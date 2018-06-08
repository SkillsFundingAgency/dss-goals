﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NCS.DSS.Goal.GetGoalByIdHttpTrigger
{
    public class GetGoalByIdHttpTriggerService
    {
        public async Task<Models.Goal> GetGoal(Guid goalId)
        {
            var goals = CreateTempGoals();
            var result = goals.FirstOrDefault(a => a.GoalId == goalId);
            return await Task.FromResult(result);
        }

        public List<Models.Goal> CreateTempGoals()
        {
            var goalsList = new List<Models.Goal>
            {
                new Models.Goal
                {
                    GoalId = Guid.Parse("489cc04f-399f-41cb-9afe-1934884f3c5f"),
                    CustomerId = Guid.NewGuid(),
                    ActionPlanId = Guid.NewGuid(),
                    DateGoalAgreed = DateTime.Today.AddDays(-5),
                    DateGoalAimsToBeCompletedBy = DateTime.Today.AddDays(10),
                    DateGoalActuallyCompleted = DateTime.Today.AddDays(12),
                    GoalSummary = "This is a fake summary",
                    GoalTypeId = 1,
                    GoalStatusId = 1,
                    PersonResponsibleId = 1,
                    LastModifiedDate = DateTime.Today.AddYears(1),
                    LastModifiedBy = Guid.NewGuid()
                },
                new Models.Goal
                {
                    GoalId = Guid.Parse("4221d30e-1d56-42dd-bae9-2f20e519b261"),
                    CustomerId = Guid.NewGuid(),
                    ActionPlanId = Guid.NewGuid(),
                    DateGoalAgreed = DateTime.Today,
                    DateGoalAimsToBeCompletedBy = DateTime.Today.AddDays(5),
                    DateGoalActuallyCompleted = DateTime.Today.AddDays(5),
                    GoalSummary = "This is a fake summary v2",
                    GoalTypeId = 2,
                    GoalStatusId = 2,
                    PersonResponsibleId = 2,
                    LastModifiedDate = DateTime.Today.AddYears(1),
                    LastModifiedBy = Guid.NewGuid()
                },
                new Models.Goal
                {
                    GoalId = Guid.Parse("bc5ac80d-f820-4cd8-8505-548c9c9db5a5"),
                    CustomerId = Guid.NewGuid(),
                    ActionPlanId = Guid.NewGuid(),
                    DateGoalAgreed = DateTime.Today.AddDays(-20),
                    DateGoalAimsToBeCompletedBy = DateTime.Today.AddDays(2),
                    DateGoalActuallyCompleted = DateTime.Today.AddDays(1),
                    GoalSummary = "This is a fake summary v3",
                    GoalTypeId = 3,
                    GoalStatusId = 3,
                    PersonResponsibleId = 3,
                    LastModifiedDate = DateTime.Today,
                    LastModifiedBy = Guid.NewGuid()
                }

            };

            return goalsList;
        }
    }
}