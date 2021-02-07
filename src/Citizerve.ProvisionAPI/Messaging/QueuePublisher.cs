using Citizerve.ProvisionAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System.Text;

namespace Citizerve.ProvisionAPI.Messaging
{
    public class QueuePublisher : IQueuePublisher
    {
        private readonly QueueClient _provisionQueueClient;

        public QueuePublisher(IQueueSettings settings)
        {
            _provisionQueueClient = new QueueClient(settings.ConnectionString, settings.ProvisionQueueName);
        }

        public async Task PublishProvisionResource (Resource resource)
        {
            string citizenMessage = JsonConvert.SerializeObject(resource);

            await _provisionQueueClient.SendAsync(new Message(Encoding.UTF8.GetBytes(citizenMessage)));
        }
    }
}
