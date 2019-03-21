using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace NCS.DSS.Goal.GoalChangeFeedTrigger.Service
{
    public interface IGoalChangeFeedTriggerService
    {
        Task SendMessageToChangeFeedQueueAsync(Document document);
    }
}
