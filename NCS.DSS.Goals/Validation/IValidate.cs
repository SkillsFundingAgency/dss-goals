using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Goals.Models;

namespace NCS.DSS.Goals.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(IGoal resource, bool validateModelForPost);
    }
}