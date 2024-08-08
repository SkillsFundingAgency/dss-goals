﻿using NCS.DSS.Goal.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Goal.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(IGoal resource, bool validateModelForPost);
    }
}