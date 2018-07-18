﻿using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Goal.Cosmos.Provider;

namespace NCS.DSS.Goal.PostGoalHttpTrigger.Service
{
    public class PostGoalHttpTriggerService : IPostGoalHttpTriggerService
    {
        public async Task<Models.Goal> CreateAsync(Models.Goal goal)
        {
            if (goal == null)
                return null;

            var goalId = Guid.NewGuid();
            goal.GoalId = goalId;

            if (!goal.LastModifiedDate.HasValue)
                goal.LastModifiedDate = DateTime.Now;

            var documentDbProvider = new DocumentDBProvider();

            var response = await documentDbProvider.CreateGoalAsync(goal);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : null;
        }
    }
}