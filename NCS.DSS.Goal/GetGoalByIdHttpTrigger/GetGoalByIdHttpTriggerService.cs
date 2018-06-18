using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NCS.DSS.Goal.ReferenceData;

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
                    DateGoalCaptured = DateTime.Today.AddDays(-5),
                    DateGoalShouldBeCompletedBy = DateTime.Today.AddDays(10),
                    DateGoalAchieved = DateTime.Today.AddDays(12),
                    GoalSummary = "This is a fake summary",
                    GoalType = GoalType.Learning,
                    GoalStatus = GoalStatus.Achieved,
                    LastModifiedDate = DateTime.Today.AddYears(1),
                    LastModifiedBy = Guid.NewGuid()
                },
                new Models.Goal
                {
                    GoalId = Guid.Parse("4221d30e-1d56-42dd-bae9-2f20e519b261"),
                    CustomerId = Guid.NewGuid(),
                    ActionPlanId = Guid.NewGuid(),
                    DateGoalCaptured = DateTime.Today,
                    DateGoalShouldBeCompletedBy = DateTime.Today.AddDays(5),
                    DateGoalAchieved = DateTime.Today.AddDays(5),
                    GoalSummary = "This is a fake summary v2",
                    GoalType = GoalType.Other,
                    GoalStatus = GoalStatus.InProgress,
                    LastModifiedDate = DateTime.Today.AddYears(1),
                    LastModifiedBy = Guid.NewGuid()
                },
                new Models.Goal
                {
                    GoalId = Guid.Parse("bc5ac80d-f820-4cd8-8505-548c9c9db5a5"),
                    CustomerId = Guid.NewGuid(),
                    ActionPlanId = Guid.NewGuid(),
                    DateGoalCaptured = DateTime.Today.AddDays(-20),
                    DateGoalShouldBeCompletedBy = DateTime.Today.AddDays(2),
                    DateGoalAchieved = DateTime.Today.AddDays(1),
                    GoalSummary = "This is a fake summary v3",
                    GoalType = GoalType.Skills,
                    GoalStatus = GoalStatus.NoLongerRelevant,
                    LastModifiedDate = DateTime.Today,
                    LastModifiedBy = Guid.NewGuid()
                }

            };

            return goalsList;
        }
    }
}