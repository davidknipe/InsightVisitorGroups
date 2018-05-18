using System.ServiceModel;
using InsightVisitorGroups.Mailing;

namespace InsightVisitorGroups.Impl
{
    internal class ServiceClientFactory
    {
        private readonly string baseApiUrl = "https://api.campaign.episerver.net";

        public MailingWebserviceClient GetMailingClient()
        {
            return new MailingWebserviceClient(
                new BasicHttpBinding(BasicHttpSecurityMode.Transport),
                new EndpointAddress(baseApiUrl + "/soap11/Mailing"));
        }
    }
}