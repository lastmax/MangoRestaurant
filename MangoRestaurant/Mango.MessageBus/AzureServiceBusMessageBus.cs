using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mango.MessageBus
{
    public class AzureServiceBusMessageBus : IMessageBus
    {
        private string _connectionString;

        public AzureServiceBusMessageBus(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task PublishMessage(BaseMessage message, string topicName)
        {
            try
            {
                await using ServiceBusClient client = new(_connectionString);
                var sender = client.CreateSender(topicName);

                var jsonMessage = JsonConvert.SerializeObject(message);
                var finalMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMessage))
                {
                    CorrelationId = Guid.NewGuid().ToString()
                };

                await sender.SendMessageAsync(finalMessage);
                await client.DisposeAsync();
            }
            catch (Exception ex)
            {
                var erMessage = ex.Message;
            }
            
        }
    }
}
