using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace NCS.DSS.Goals.GoalChangeFeedTrigger.Service
{
    public interface IGoalChangeFeedTriggerService
    {
        Task SendMessageToChangeFeedQueueAsync(Document document);
    }
}
