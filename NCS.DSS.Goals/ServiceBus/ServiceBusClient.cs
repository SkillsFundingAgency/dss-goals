using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace NCS.DSS.Goals.ServiceBus
{
    public static class ServiceBusClient
    {
        public static readonly string QueueName = Environment.GetEnvironmentVariable("QueueName");
        public static readonly string ServiceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");

        public static async Task SendPostMessageAsync(Models.Goal Goals, string reqUrl)
        {
            var queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            var messageModel = new MessageModel()
            {
                TitleMessage = "New Goals record {" + Goals.GoalId + "} added at " + DateTime.UtcNow,
                CustomerGuid = Goals.CustomerId,
                LastModifiedDate = Goals.LastModifiedDate,
                URL = reqUrl + "/" + Goals.GoalId,
                IsNewCustomer = false,
                TouchpointId = Goals.LastModifiedBy
            };

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = Goals.CustomerId + " " + DateTime.UtcNow
            };

            await queueClient.SendAsync(msg);
        }

        public static async Task SendPatchMessageAsync(Models.Goal Goals, Guid customerId, string reqUrl)
        {
            var queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            var messageModel = new MessageModel
            {
                TitleMessage = "Goals record modification for {" + customerId + "} at " + DateTime.UtcNow,
                CustomerGuid = customerId,
                LastModifiedDate = Goals.LastModifiedDate,
                URL = reqUrl,
                IsNewCustomer = false,
                TouchpointId = Goals.LastModifiedBy
            };

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = customerId + " " + DateTime.UtcNow
            };

            await queueClient.SendAsync(msg);
        }
    }

    public class MessageModel
    {
        public string TitleMessage { get; set; }
        public Guid? CustomerGuid { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string URL { get; set; }
        public bool IsNewCustomer { get; set; }
        public string TouchpointId { get; set; }
    }
}