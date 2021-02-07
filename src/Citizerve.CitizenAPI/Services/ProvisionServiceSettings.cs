namespace Citizerve.CitizenAPI.Services
{
    public class ProvisionServiceSettings : IProvisionServiceSettings
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
