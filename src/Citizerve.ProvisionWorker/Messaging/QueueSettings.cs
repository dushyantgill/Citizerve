namespace Citizerve.ProvisionWorker.Messaging
{
    public class QueueSettings : IQueueSettings
    {
        public string ConnectionString { get; set; }
        public string ProvisionQueueName { get; set; }
    }

    public interface IQueueSettings
    {
        string ConnectionString { get; set; }
        string ProvisionQueueName { get; set; }
    }
}
