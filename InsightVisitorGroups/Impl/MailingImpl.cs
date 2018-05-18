using System;
using System.Collections.Generic;
using EPiServer.ConnectForCampaign.Core.Configuration;
using EPiServer.ConnectForCampaign.Services.Implementation;
using EPiServer.ServiceLocation;
using NuGet;

namespace InsightVisitorGroups.Impl
{
    [ServiceConfiguration(typeof(IMailing))]
    public class MailingImpl : IMailing
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ICampaignSettings _campaignSettings;

        public MailingImpl(IAuthenticationService authenticationService, ICampaignSettings campaignSettings)
        {
            _authenticationService = authenticationService;
            _campaignSettings = campaignSettings;
        }

        public IList<Tuple<long, string, string>> GetActiveMailings()
        {
            var clientFactory = new ServiceClientFactory();
            var mailingClient = clientFactory.GetMailingClient();

            var token = GetToken();
            var getDataSetFlat = mailingClient.getDataSetFlat(token, "regular");
            //var eventMails = mailingClient.getDataSetFlat(token, "confirmation");
           // getDataSetFlat.AddRange(eventMails);
            var getColumnNames = mailingClient.getColumnNames(token, "regular");
            var returnValues = new List<Tuple<long, string, string>>();

            long mailingId = 0;
            string mailingName = string.Empty;
            string mailingDescription = string.Empty;

            var numCols = getColumnNames.Length;
            int i = 0;
            foreach (var dataItem in getDataSetFlat)
            {
                if (i == numCols)
                {
                    if (mailingId != 0)
                    {
                        returnValues.Add(new Tuple<long, string, string>(mailingId, mailingName, mailingDescription));
                        mailingId = 0;
                        mailingName = string.Empty;
                        mailingDescription = string.Empty;
                    }            
                    i = 0;
                }
                switch (getColumnNames[i])
                {
                    case "ID":
                        mailingId = long.Parse(dataItem);
                        break;
                    case "Name":
                        mailingName = dataItem;
                        break;
                    case "Beschreibung":
                        mailingDescription = dataItem;
                        break;
                }

                i++;
            }
            //var getDescription = mailingClient.getDescription(token, 1234);

            return returnValues;
        }

        private string GetToken()
        {
            return _authenticationService.GetToken(
                _campaignSettings.MandatorId,
                _campaignSettings.UserName,
                _campaignSettings.Password, false);
        }
    }
}
