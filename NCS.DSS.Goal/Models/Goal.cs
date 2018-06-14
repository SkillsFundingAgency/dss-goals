using System;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Goal.Models
{
    public class Goal
    {
        [Display(Description = "")]
        public Guid GoalId { get; set; }

        [Required]
        [Display(Description = "Unique identifier for a subscription record. ")]
        public Guid CustomerId { get; set; }

        [Required]
        [Display(Description = "Unique identifier for the customers action plan.")]
        public Guid ActionPlanId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the goal was captured or agreed with the customer")]
        public DateTime DateGoalCaptured { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date the customer aims to complete their goal by.")]
        public DateTime DateGoalShouldBeCompletedBy { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date the customer actually achieved their goal")]
        public DateTime DateGoalAchieved { get; set; }

        [Display(Description = "Details of the customer goal.")]
        public string GoalSummary { get; set; }

        [Display(Description = " GoalType reference data")]
        public int GoalTypeId { get; set; }

        [Display(Description = "Goal status reference data")]
        public int GoalStatusId { get ; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        public DateTime LastModifiedDate { get; set; }

        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        public Guid LastModifiedBy { get; set; }
    }
}