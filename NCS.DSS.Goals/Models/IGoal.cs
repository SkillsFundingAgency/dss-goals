using System;
using NCS.DSS.Goals.ReferenceData;

namespace NCS.DSS.Goals.Models
{
    public interface IGoal
    {
        DateTime? DateGoalCaptured { get; set; }
        DateTime? DateGoalShouldBeCompletedBy { get; set; }
        DateTime? DateGoalAchieved { get; set; }
        string GoalSummary { get; set; }
        GoalType? GoalType { get; set; }
        GoalStatus? GoalStatus { get; set; }
        DateTime? LastModifiedDate { get; set; }
        string LastModifiedBy { get; set; }

        void SetDefaultValues();
    }
}