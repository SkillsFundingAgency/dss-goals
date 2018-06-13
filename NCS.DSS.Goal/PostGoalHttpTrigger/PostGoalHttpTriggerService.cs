using System;

namespace NCS.DSS.Goal.PostGoalHttpTrigger
{
    public class PostGoalHttpTriggerService
    {
        public Guid? Create(Models.Goal goal)
        {
            if (goal == null)
                return null;

            return Guid.NewGuid();
        }
    }
}