using System;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Goal.Models
{
    public class Goal
    {
        public Guid GoalId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public Guid ActionPlanId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateGoalAgreed { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateGoalAimsToBeCompletedBy { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateGoalActuallyCompleted { get; set; }

        public string GoalSummary { get; set; }

        public int GoalTypeId { get; set; }

        public int GoalStatusId { get; set; }

        public int PersonResponsibleId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime LastModifiedDate { get; set; }

        public Guid LastModifiedBy { get; set; }
    }
}