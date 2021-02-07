using Citizerve.ProvisionWorker.Messaging;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Citizerve.ProvisionWorker
{
    public class ProvisionWorker : BackgroundService
    {
        private readonly ILogger<ProvisionWorker> _logger;
        private readonly QueueClient _queueClient;

        public ProvisionWorker(ILogger<ProvisionWorker> logger, IQueueSettings settings)
        {
            _queueClient = new QueueClient(settings.ConnectionString, settings.ProvisionQueueName);
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            //process upto 5 messages at the same time
            var messageHandlerOptions = new MessageHandlerOptions(HandleFailure)
            {
                MaxConcurrentCalls = 5,
                AutoComplete = false,
                MaxAutoRenewDuration = TimeSpan.FromMinutes(10)
            };

            _queueClient.RegisterMessageHandler(HandleMessage, messageHandlerOptions);

            await Task.Delay(1000);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await _queueClient.CloseAsync().ConfigureAwait(false);
        }

        public async Task HandleMessage(Message message, CancellationToken cancelToken)
        {
            var body = Encoding.UTF8.GetString(message.Body);

            //simulate processing by delaying 30-60 seconds
            await Task.Delay(1000 * new Random().Next(30, 60));

            await _queueClient.CompleteAsync(message.SystemProperties.LockToken).ConfigureAwait(false);
        }

        public virtual Task HandleFailure(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            return Task.CompletedTask;
        }
    }
}
