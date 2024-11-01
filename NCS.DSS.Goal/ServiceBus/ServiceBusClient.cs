﻿using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System.Text;

namespace NCS.DSS.Goal.ServiceBus
{
    public static class ServiceBusClient
    {
        public static readonly string QueueName = Environment.GetEnvironmentVariable("QueueName");
        public static readonly string ServiceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");

        public static async Task SendPostMessageAsync(Models.Goal goals, string reqUrl)
        {
            var queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            var messageModel = new MessageModel()
            {
                TitleMessage = "New Goals record {" + goals.GoalId + "} added at " + DateTime.UtcNow,
                CustomerGuid = goals.CustomerId,
                LastModifiedDate = goals.LastModifiedDate,
                URL = reqUrl + "/" + goals.GoalId,
                IsNewCustomer = false,
                TouchpointId = goals.LastModifiedBy
            };

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = goals.CustomerId + " " + DateTime.UtcNow
            };

            await queueClient.SendAsync(msg);
        }

        public static async Task SendPatchMessageAsync(Models.Goal goals, Guid customerId, string reqUrl)
        {
            var queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            var messageModel = new MessageModel
            {
                TitleMessage = "Goals record modification for {" + customerId + "} at " + DateTime.UtcNow,
                CustomerGuid = customerId,
                LastModifiedDate = goals.LastModifiedDate,
                URL = reqUrl,
                IsNewCustomer = false,
                TouchpointId = goals.LastModifiedBy
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