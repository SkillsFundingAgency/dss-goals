using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Goal.Models;
using NCS.DSS.Goal.ReferenceData;

namespace NCS.DSS.Goal.Validation
{
    public class Validate : IValidate
    {
        public List<ValidationResult> ValidateResource(IGoal resource)
        {
            var context = new ValidationContext(resource, null, null);
            var results = new List<ValidationResult>();

             Validator.TryValidateObject(resource, context, results, true);
            ValidateGoalRules(resource, results);

            return results;
        }

        private void ValidateGoalRules(IGoal goalResource, List<ValidationResult> results)
        {
            if (goalResource == null)
                return;

            if (string.IsNullOrWhiteSpace(goalResource.GoalSummary))
                results.Add(new ValidationResult("Goal Summary is a required field", new[] { "GoalSummary" }));

            if (goalResource.DateGoalCaptured.HasValue && goalResource.DateGoalCaptured.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Date Goal Captured must be less the current date/time", new[] { "DateGoalCaptured" }));

            if (goalResource.DateGoalAchieved.HasValue && goalResource.DateGoalAchieved.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Date Goal Achieved must be less the current date/time", new[] { "DateGoalAchieved" }));

            if (goalResource.LastModifiedDate.HasValue && goalResource.LastModifiedDate.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Last Modified Date must be less the current date/time", new[] { "LastModifiedDate" }));

            if (goalResource.GoalType.HasValue && !Enum.IsDefined(typeof(GoalType), goalResource.GoalType.Value))
                results.Add(new ValidationResult("Please supply a valid Goal Type", new[] { "GoalType" }));

            if (goalResource.GoalStatus.HasValue && !Enum.IsDefined(typeof(GoalStatus), goalResource.GoalStatus.Value))
                results.Add(new ValidationResult("Please supply a valid Goal Status", new[] { "GoalStatus" }));

        }

    }
}
