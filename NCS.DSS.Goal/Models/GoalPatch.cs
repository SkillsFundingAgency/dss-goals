﻿using System;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Goal.Annotations;
using NCS.DSS.Goal.ReferenceData;

namespace NCS.DSS.Goal.Models
{
    public class GoalPatch
    {
        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the goal was captured or agreed with the customer")]
        [Example(Description = "2018-06-21T11:31:00")]
        public DateTime? DateGoalCaptured { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date the customer aims to complete their goal by.")]
        [Example(Description = "2018-06-23T12:01:00")]
        public DateTime? DateGoalShouldBeCompletedBy { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date the customer actually achieved their goal")]
        [Example(Description = "2018-06-22T19:53:00")]
        public DateTime? DateGoalAchieved { get; set; }

        [StringLength(2000)]
        [Display(Description = "Details of the customer goal.")]
        [Example(Description = "this is some text")]
        public string GoalSummary { get; set; }

        [Display(Description = " GoalType reference data")]
        [Example(Description = "1")]
        public GoalType? GoalType { get; set; }

        [Display(Description = "Goal status reference data")]
        [Example(Description = "2")]
        public GoalStatus? GoalStatus { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        [Example(Description = "2018-06-24T01:01:00")]
        public DateTime? LastModifiedDate { get; set; }

        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "d1307d77-af23-4cb4-b600-a60e04f8c3df")]
        public Guid? LastModifiedBy { get; set; }
    }
}