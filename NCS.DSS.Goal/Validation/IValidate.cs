using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Goal.Models;

namespace NCS.DSS.Goal.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(IGoal resource, bool validateModelForPost);
    }
}