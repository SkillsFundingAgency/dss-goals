using System;
using System.ComponentModel.DataAnnotations;
using DFC.JSON.Standard.Attributes;
using DFC.Swagger.Standard.Annotations;
using NCS.DSS.Goal.ReferenceData;

namespace NCS.DSS.Goal.Models
{
    public class Goal : IGoal
    {
        [Display(Description = "Unique identifier for a goal record")]
        [Example(Description = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
        public Guid? GoalId { get; set; }

        [Display(Description = "Unique identifier for a subscription record. ")]
        [Example(Description = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85")]
        public Guid? CustomerId { get; set; }

        [Display(Description = "Unique identifier for the customers goal plan.")]
        [Example(Description = "2730af9c-fc34-4c2b-a905-c4b584b0f379")]
        public Guid? ActionPlanId { get; set; }

        [StringLength(50)]
        [Display(Description = "Identifier supplied by the touchpoint to indicate their subcontractor")]
        [Example(Description = "01234567899876543210")]
        public string SubcontractorId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the goal was captured or agreed with the customer")]
        [Example(Description = "2018-06-21T11:31:00")]
        public DateTime? DateGoalCaptured { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Description = "Date the customer aims to complete their goal by.")]
        [Example(Description = "2018-06-23T12:01:00")]
        public DateTime? DateGoalShouldBeCompletedBy { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date the customer actually achieved their goal")]
        [Example(Description = "2018-06-22T19:53:00")]
        public DateTime? DateGoalAchieved { get; set; }

        [Required]
        [StringLength(2000)]
        [Display(Description = "Details of the customer goal")]
        [Example(Description = "this is some text")]
        public string GoalSummary { get; set; }

        [Required]
        [Display(Description = " GoalType reference data.")]
        [Example(Description = "1")]
        public GoalType? GoalType { get; set; }

        [Display(Description = "Goal status reference data.")]
        [Example(Description = "2")]
        public GoalStatus? GoalStatus { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        [Example(Description = "2018-06-24T01:01:00")]
        public DateTime? LastModifiedDate { get; set; }

        [StringLength(10, MinimumLength = 10)]
        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "0000000001")]
        public string LastModifiedBy { get; set; }

        [JsonIgnoreOnSerialize]
        public string CreatedBy { get; set; }


        public void SetDefaultValues()
        {
            if (!LastModifiedDate.HasValue)
                LastModifiedDate = DateTime.UtcNow;

            if (GoalType == null)
                GoalType = ReferenceData.GoalType.Other;

            if (GoalStatus == null)
                GoalStatus = ReferenceData.GoalStatus.InProgress;
        }

        public void SetIds(Guid customerId, Guid actionPlanId, string touchpointId, string subcontractorId)
        {
            GoalId = Guid.NewGuid();
            CustomerId = customerId;
            ActionPlanId = actionPlanId;
            LastModifiedBy = touchpointId;
            SubcontractorId = subcontractorId;
            CreatedBy = touchpointId;
        }

    }
}