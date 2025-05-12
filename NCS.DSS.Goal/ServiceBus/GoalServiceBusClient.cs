using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Goal.Models;
using Newtonsoft.Json;
using System.Text;

namespace NCS.DSS.Goal.ServiceBus
{
    public class GoalServiceBusClient : IGoalServiceBusClient
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ILogger<GoalServiceBusClient> _logger;
        private readonly string _queueName;

        public GoalServiceBusClient(ServiceBusClient serviceBusClient,
            IOptions<GoalConfigurationSettings> configOptions,
            ILogger<GoalServiceBusClient> logger)
        {
            var config = configOptions.Value;
            if (string.IsNullOrEmpty(config.QueueName))
            {
                throw new ArgumentNullException(nameof(config.QueueName), "QueueName cannot be null or empty.");
            }

            _serviceBusClient = serviceBusClient;
            _queueName = config.QueueName;
            _logger = logger;
        }

        public async Task SendPostMessageAsync(Models.Goal goals, string reqUrl)
        {
            var serviceBusSender = _serviceBusClient.CreateSender(_queueName);

            var messageModel = new MessageModel()
            {
                TitleMessage = "New Goals record {" + goals.GoalId + "} added at " + DateTime.UtcNow,
                CustomerGuid = goals.CustomerId,
                LastModifiedDate = goals.LastModifiedDate,
                URL = reqUrl + "/" + goals.GoalId,
                IsNewCustomer = false,
                TouchpointId = goals.LastModifiedBy
            };

            var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = goals.CustomerId + " " + DateTime.UtcNow
            };

            _logger.LogInformation("Attempting to send POST message to service bus. Goal ID: {GoalId}", goals.GoalId);

            await serviceBusSender.SendMessageAsync(msg);

            _logger.LogInformation("Successfully sent POST message to the service bus. Goal ID: {GoalId}", goals.GoalId);
        }

        public async Task SendPatchMessageAsync(Models.Goal goals, Guid customerId, string reqUrl)
        {
            var serviceBusSender = _serviceBusClient.CreateSender(_queueName);

            var messageModel = new MessageModel
            {
                TitleMessage = "Goals record modification for {" + customerId + "} at " + DateTime.UtcNow,
                CustomerGuid = customerId,
                LastModifiedDate = goals.LastModifiedDate,
                URL = reqUrl,
                IsNewCustomer = false,
                TouchpointId = goals.LastModifiedBy
            };

            var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = customerId + " " + DateTime.UtcNow
            };

            _logger.LogInformation("Attempting to send PATCH message to service bus. Goal ID: {GoalId}", goals.GoalId);

            await serviceBusSender.SendMessageAsync(msg);

            _logger.LogInformation("Successfully sent PATCH message to the service bus. Goal ID: {GoalId}", goals.GoalId);
        }
    }
}