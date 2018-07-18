using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Goal.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource<T>(T resource);
    }
}