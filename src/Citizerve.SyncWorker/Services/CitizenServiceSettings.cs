namespace Citizerve.SyncWorker.Services
{
    public class CitizenServiceSettings : IProvisionServiceSettings
    {
        public string Url { get; set; }
        public string ApiVersion { get; set; }
    }

    public interface IProvisionServiceSettings
    {
        string Url { get; set; }
        string ApiVersion { get; set; }
    }
}
